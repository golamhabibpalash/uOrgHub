using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using uOrgHub.Shared.Data;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Models.Enums;
using uOrgHub.Sms.Providers;

namespace uOrgHub.Sms.Services;

public interface ISmsDispatcher
{
    /// <summary>Sends queued messages that are due. Returns how many were attempted.</summary>
    Task<int> ProcessQueuedAsync(int batchSize, CancellationToken ct);

    /// <summary>Polls delivery reports for submitted messages on providers that have them.</summary>
    Task<int> CheckDeliveryAsync(int batchSize, CancellationToken ct);

    /// <summary>
    /// Messages left in Sending by a crash mid-call: their fate is unknown, so they become
    /// Undetermined rather than being sent again.
    /// </summary>
    Task<int> RecoverInterruptedAsync(CancellationToken ct);
}

/// <summary>
/// Moves messages from Queued to a final state. Runs outside any request, so the company filter
/// is off and it serves every company; each message carries its own gateway.
///
/// The rules that keep a recipient from getting two copies:
/// * one message is claimed (Queued → Sending, saved) before its provider call, never a batch;
/// * only a failure that certainly did not reach the vendor is retried (<see cref="SmsSendOutcome.Retryable"/>);
/// * a timeout is <see cref="SmsMessageStatus.Undetermined"/> and is left for a person to judge.
/// </summary>
public sealed class SmsDispatcher(
    AppDbContext db,
    ISmsGatewayRuntime runtime,
    ISmsProviderRegistry registry,
    ILogger<SmsDispatcher> logger) : ISmsDispatcher
{
    public const int MaxAttempts = 3;
    private static readonly TimeSpan FirstDeliveryCheck = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan DeliveryCheckWindow = TimeSpan.FromDays(3);
    private const int MaxDeliveryChecks = 12;

    public async Task<int> ProcessQueuedAsync(int batchSize, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var due = await db.Set<SmsMessage>()
            .Where(m => !m.IsDeleted && m.Status == SmsMessageStatus.Queued && (m.NextAttemptAt == null || m.NextAttemptAt <= now))
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.Id)
            .Take(batchSize)
            .ToListAsync(ct);

        var contexts = new Dictionary<Guid, (SmsGateway Gateway, ISmsProvider? Provider, SmsProviderContext? Context, string? Error)>();
        var attempted = 0;

        foreach (var id in due)
        {
            ct.ThrowIfCancellationRequested();
            var message = await db.Set<SmsMessage>().Include(m => m.Gateway).FirstAsync(m => m.Id == id, ct);
            if (message.Status != SmsMessageStatus.Queued) continue;

            if (!contexts.TryGetValue(message.GatewayId, out var gw))
            {
                gw = Prepare(message.Gateway);
                contexts[message.GatewayId] = gw;
            }

            if (gw.Error is not null)
            {
                Fail(message, "GATEWAY", gw.Error);
                await db.SaveChangesAsync(ct);
                continue;
            }

            // Claim before calling out, so a crash during the call can't lead to a second send.
            message.Status = SmsMessageStatus.Sending;
            message.Attempts++;
            await db.SaveChangesAsync(ct);

            SmsSendResult result;
            try
            {
                result = SmsResultRedaction.Redact(await gw.Provider!.SendAsync(gw.Context!,
                    new SmsOutgoingMessage(message.Recipient, message.MessageText, message.Encoding, message.Segments), ct), gw.Context!);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A provider bug, not a network fault — whether it sent is unknown.
                logger.LogError(ex, "SMS provider {Provider} threw while sending message {MessageId}", message.ProviderKey, message.Id);
                result = new SmsSendResult(SmsSendOutcome.Undetermined, ErrorCode: "PROVIDER_ERROR", ErrorMessage: "The provider integration failed unexpectedly.");
            }

            Apply(message, result, gw.Gateway);
            await db.SaveChangesAsync(ct);
            attempted++;

            if (result.Outcome != SmsSendOutcome.Accepted)
                logger.LogWarning("SMS {MessageId} via {Gateway}: {Outcome} {Code} {Error}",
                    message.Id, message.GatewayName, result.Outcome, result.ErrorCode, result.ErrorMessage);

            // Stay inside the vendor's rate limit during a bulk send.
            var perSecond = Math.Max(1, gw.Gateway.MaxSendsPerSecond);
            if (perSecond < 100) await Task.Delay(TimeSpan.FromMilliseconds(1000.0 / perSecond), ct);
        }

        return attempted;
    }

    private (SmsGateway, ISmsProvider?, SmsProviderContext?, string?) Prepare(SmsGateway gateway)
    {
        if (gateway.IsDeleted) return (gateway, null, null, $"Gateway '{gateway.Name}' was removed before this message was sent.");
        if (!gateway.IsEnabled) return (gateway, null, null, $"Gateway '{gateway.Name}' is disabled.");
        if (!registry.Exists(gateway.ProviderKey)) return (gateway, null, null, $"SMS provider '{gateway.ProviderKey}' is not available.");
        try
        {
            return (gateway, runtime.Provider(gateway), runtime.CreateContext(gateway), null);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return (gateway, null, null, $"Gateway '{gateway.Name}' credentials cannot be decrypted (the encryption key changed). Re-enter them.");
        }
    }

    internal static void Apply(SmsMessage message, SmsSendResult result, SmsGateway gateway)
    {
        var now = DateTime.UtcNow;
        message.ProviderResponse = result.RawResponse;
        message.ProviderStatus = result.ProviderStatus ?? message.ProviderStatus;

        switch (result.Outcome)
        {
            case SmsSendOutcome.Accepted:
                message.Status = SmsMessageStatus.Submitted;
                message.SubmittedAt = now;
                message.ProviderMessageId = result.ProviderMessageId;
                message.ErrorCode = null;
                message.ErrorMessage = null;
                message.NextAttemptAt = null;
                if (result.Cost is { } cost)
                {
                    message.Cost = cost;
                    message.CostIsActual = true;
                }
                else
                {
                    message.Cost = gateway.EffectiveRate * message.Segments;
                    message.CostIsActual = false;
                }
                break;

            case SmsSendOutcome.Retryable when message.Attempts < MaxAttempts:
                message.Status = SmsMessageStatus.Queued;
                message.ErrorCode = result.ErrorCode;
                message.ErrorMessage = result.ErrorMessage;
                // 30s, 60s, … or what the vendor asked for.
                message.NextAttemptAt = now + (result.RetryAfter ?? TimeSpan.FromSeconds(30 * Math.Pow(2, message.Attempts - 1)));
                break;

            case SmsSendOutcome.Undetermined:
                message.Status = SmsMessageStatus.Undetermined;
                message.ErrorCode = result.ErrorCode;
                message.ErrorMessage = (result.ErrorMessage ?? "No response from the gateway.")
                    + " It may still be delivered, so it was not retried automatically.";
                break;

            default: // Rejected, or Retryable with attempts used up
                Fail(message, result.ErrorCode, result.ErrorMessage
                    + (result.Outcome == SmsSendOutcome.Retryable ? $" Gave up after {message.Attempts} attempts." : ""));
                break;
        }
    }

    private static void Fail(SmsMessage message, string? code, string? error)
    {
        message.Status = SmsMessageStatus.Failed;
        message.FailedAt = DateTime.UtcNow;
        message.ErrorCode = code;
        message.ErrorMessage = string.IsNullOrWhiteSpace(error) ? "Sending failed." : error.Trim();
        message.NextAttemptAt = null;
    }

    public async Task<int> CheckDeliveryAsync(int batchSize, CancellationToken ct)
    {
        var withReports = registry.Descriptors.Where(d => d.Capabilities.DeliveryReports).Select(d => d.Key).ToList();
        if (withReports.Count == 0) return 0;

        var now = DateTime.UtcNow;
        var submittedBefore = now - FirstDeliveryCheck;
        var oldest = now - DeliveryCheckWindow;
        var candidates = await db.Set<SmsMessage>().Include(m => m.Gateway)
            .Where(m => !m.IsDeleted && m.Status == SmsMessageStatus.Submitted && m.ProviderMessageId != null
                && withReports.Contains(m.ProviderKey)
                && m.SubmittedAt <= submittedBefore && m.SubmittedAt >= oldest
                && m.StatusChecks < MaxDeliveryChecks)
            .OrderBy(m => m.LastStatusCheckAt ?? m.SubmittedAt)
            .Take(batchSize * 4)
            .ToListAsync(ct);

        // Back off per message: 1, 2, 4 … 60 minutes between checks.
        var due = candidates
            .Where(m => m.LastStatusCheckAt is null
                || m.LastStatusCheckAt <= now - TimeSpan.FromMinutes(Math.Min(60, Math.Pow(2, m.StatusChecks))))
            .Take(batchSize)
            .ToList();

        var contexts = new Dictionary<Guid, (SmsGateway, ISmsProvider?, SmsProviderContext?, string?)>();
        foreach (var message in due)
        {
            if (!contexts.TryGetValue(message.GatewayId, out var gw))
                contexts[message.GatewayId] = gw = Prepare(message.Gateway);
            var (_, provider, context, error) = gw;

            message.StatusChecks++;
            message.LastStatusCheckAt = now;
            if (error is null)
            {
                try
                {
                    var report = SmsResultRedaction.Redact(
                        await provider!.GetDeliveryStatusAsync(context!, message.ProviderMessageId!, message.Recipient, ct), context!);
                    if (report.Supported && report.Error is null)
                    {
                        message.ProviderStatus = report.ProviderStatus ?? message.ProviderStatus;
                        if (report.Cost is { } cost)
                        {
                            message.Cost = cost;
                            message.CostIsActual = true;
                        }
                        if (report.Status == SmsMessageStatus.Delivered)
                        {
                            message.Status = SmsMessageStatus.Delivered;
                            message.DeliveredAt = now;
                        }
                        else if (report.Status == SmsMessageStatus.Failed)
                        {
                            Fail(message, "UNDELIVERED", $"Provider reported '{report.ProviderStatus}'.");
                        }
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Delivery report check failed for SMS {MessageId}", message.Id);
                }
            }
            await db.SaveChangesAsync(ct);
        }
        return due.Count;
    }

    public async Task<int> RecoverInterruptedAsync(CancellationToken ct)
    {
        var stuck = await db.Set<SmsMessage>().Where(m => !m.IsDeleted && m.Status == SmsMessageStatus.Sending).ToListAsync(ct);
        foreach (var message in stuck)
        {
            message.Status = SmsMessageStatus.Undetermined;
            message.ErrorCode = "INTERRUPTED";
            message.ErrorMessage = "The server stopped while this message was being sent. It may have been delivered, so it was not retried automatically.";
        }
        if (stuck.Count > 0) await db.SaveChangesAsync(ct);
        return stuck.Count;
    }
}
