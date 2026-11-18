using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Services;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.DTOs.Validators;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Models.Enums;

namespace uOrgHub.Sms.Services;

/// <summary>
/// The one way a send is created — compose page, gateway test, or another module via
/// <see cref="ISmsSender"/>. Validates and normalises recipients, prices the send, writes the
/// request and one queued message per recipient, then wakes the dispatcher. Nothing here talks
/// to a provider: the request returns as soon as the work is safely stored.
/// </summary>
public sealed class SmsSendService(AppDbContext db, SmsDispatchSignal signal, ICurrentCompanyAccessor company) : ISmsSender
{
    public async Task<(SmsSendRequest Request, bool Duplicate)> CreateAsync(
        IEnumerable<string> recipients, string message, string idempotencyKey, string source,
        Guid? userId, Guid? gatewayId = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new AppException("An idempotency key is required.");

        var existing = await FindByKeyAsync(idempotencyKey, ct);
        if (existing is not null) return (existing, true);

        if (string.IsNullOrWhiteSpace(message))
            throw new ValidationException(["Message cannot be empty."]);
        if (message.Length > SmsLimits.MaxMessageLength)
            throw new ValidationException([$"Message cannot exceed {SmsLimits.MaxMessageLength} characters."]);

        var (valid, invalid) = SmsPhoneNumber.Parse(recipients);
        if (invalid.Count > 0)
            throw new ValidationException(invalid.Take(20).Select(n => $"'{n}' is not a valid mobile number.").ToList());
        if (valid.Count == 0)
            throw new ValidationException(["Add at least one recipient."]);
        if (valid.Count > SmsLimits.MaxRecipients)
            throw new ValidationException([$"A send can have at most {SmsLimits.MaxRecipients} recipients; this one has {valid.Count}."]);

        var gateway = gatewayId is { } id
            ? await db.Set<SmsGateway>().FirstOrDefaultAsync(g => !g.IsDeleted && g.Id == id, ct)
              ?? throw new NotFoundException(nameof(SmsGateway), id)
            : await db.Set<SmsGateway>().FirstOrDefaultAsync(g => !g.IsDeleted && g.IsDefault, ct)
              ?? throw new AppException("No active SMS gateway is set up. Configure one under SMS → Gateways and make it active.");
        if (!gateway.IsEnabled)
            throw new AppException($"SMS gateway '{gateway.Name}' is disabled.");

        var info = SmsSegmentCalculator.Calculate(message);
        var rate = gateway.EffectiveRate;
        var request = new SmsSendRequest
        {
            IdempotencyKey = idempotencyKey.Trim(),
            GatewayId = gateway.Id,
            MessageText = message,
            Encoding = info.Encoding,
            SegmentsPerRecipient = info.Segments,
            RecipientCount = valid.Count,
            TotalSegments = info.Segments * valid.Count,
            RatePerSegment = rate,
            EstimatedCost = rate * info.Segments * valid.Count,
            RequestedByUserId = userId,
            Source = source,
        };
        foreach (var number in valid)
            request.Messages.Add(new SmsMessage
            {
                GatewayId = gateway.Id,
                GatewayName = gateway.Name,
                ProviderKey = gateway.ProviderKey,
                Recipient = number,
                MessageText = message,
                Encoding = info.Encoding,
                Segments = info.Segments,
                Status = SmsMessageStatus.Queued,
            });

        db.Set<SmsSendRequest>().Add(request);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Lost a race with an identical submit (same key, unique index): hand back the winner.
            db.ChangeTracker.Clear();
            var winner = await FindByKeyAsync(idempotencyKey, ct);
            if (winner is not null) return (winner, true);
            throw;
        }

        signal.Notify();
        return (request, false);
    }

    public async Task<Guid> QueueAsync(IReadOnlyList<string> recipients, string message, string source, string idempotencyKey, CancellationToken ct = default)
    {
        if (company.CompanyId is null)
            throw new AppException("An SMS can only be queued within a company context.");
        var (request, _) = await CreateAsync(recipients, message, idempotencyKey, source, userId: null, ct: ct);
        return request.Id;
    }

    private Task<SmsSendRequest?> FindByKeyAsync(string key, CancellationToken ct)
    {
        var trimmed = key.Trim();
        return db.Set<SmsSendRequest>().Include(r => r.Gateway)
            .FirstOrDefaultAsync(r => r.IdempotencyKey == trimmed, ct);
    }
}
