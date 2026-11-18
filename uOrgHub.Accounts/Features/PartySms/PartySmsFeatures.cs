using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.PartySms;
using uOrgHub.Accounts.Features._Common;
using uOrgHub.Accounts.Features.Payment;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models.Enums;
using uOrgHub.Shared.Services;

namespace uOrgHub.Accounts.Features.PartySms;

public record GetPaymentSmsPreviewQuery(Guid PaymentId) : IQuery<PartySmsPreviewDto>;
public record SendPaymentSmsCommand(Guid PaymentId, SendPartySmsDto Dto) : ICommand<PartySmsResultDto>;
public record GetVoucherSmsPreviewQuery(Guid VoucherId) : IQuery<PartySmsPreviewDto>;
public record SendVoucherSmsCommand(Guid VoucherId, SendPartySmsDto Dto) : ICommand<PartySmsResultDto>;

/// <summary>What one document says about its SMS: who, which event, the template values.</summary>
/// <param name="Blocker">Why this document can never send (e.g. a contra voucher) — no preview either.</param>
/// <param name="SendBlocker">Why it can't send yet (e.g. a voucher not posted). The preview is still built, so
/// the post confirmation can show the message before posting.</param>
internal sealed record PartySmsSubject(
    PartyType? PartyType, Guid? PartyId, string? EventKey, string Reference,
    Dictionary<string, string> Values, string Source, string? Blocker, string? SendBlocker = null);

/// <summary>
/// Shared by payments and vouchers. The recipient is always the number on the party's own record
/// — never a number typed for the occasion — so an SMS only reaches a known contact. Preview and
/// send resolve it the same way, so what the user confirmed is what gets sent.
/// </summary>
internal static class PartySms
{
    public static string Amount(decimal amount) => amount.ToString("N2", CultureInfo.InvariantCulture);
    public static string Date(DateTime date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    /// <summary>"BankTransfer" → "Bank Transfer".</summary>
    public static string Words(string value)
        => string.Concat(value.Select((c, i) => i > 0 && char.IsUpper(c) && !char.IsUpper(value[i - 1]) ? " " + c : c.ToString()));

    public static Dictionary<string, string> Values(string party, decimal amount, DateTime date, string reference, string method) => new()
    {
        [SmsTemplatePlaceholders.Party] = party,
        [SmsTemplatePlaceholders.Amount] = Amount(amount),
        [SmsTemplatePlaceholders.Date] = Date(date),
        [SmsTemplatePlaceholders.Reference] = reference,
        [SmsTemplatePlaceholders.Method] = method,
    };

    public static async Task<(PartySmsPreviewDto Preview, PartyContact? Party)> PreviewAsync(
        PartySmsSubject subject, IPartyContactDirectory parties, ISmsSender sms, ISmsTemplateService templates, CancellationToken ct)
    {
        var dto = new PartySmsPreviewDto { PartyType = subject.PartyType, EventKey = subject.EventKey, Reference = subject.Reference };
        if (subject.Blocker is not null) return (Blocked(dto, subject.Blocker), null);
        if (subject.PartyType is null || subject.PartyId is null)
            return (Blocked(dto, "No customer, vendor or client is linked, so there is no one to text."), null);

        var party = await parties.FindAsync(subject.PartyType.Value, subject.PartyId.Value, ct);
        if (party is null) return (Blocked(dto, "The linked party no longer exists."), null);
        dto.PartyName = party.Name;

        subject.Values[SmsTemplatePlaceholders.Party] = party.Name;
        dto.Message = await templates.RenderAsync(subject.EventKey!, subject.Values, ct);

        if (string.IsNullOrWhiteSpace(party.Phone))
            return (Blocked(dto, $"{party.Name} has no phone number on file. Add one to their record to send an SMS."), party);
        var phone = sms.NormalizeRecipient(party.Phone);
        if (phone is null)
            return (Blocked(dto, $"The phone on {party.Name}'s record ({party.Phone}) is not a valid mobile number."), party);

        dto.Phone = phone;
        dto.CanSend = true;
        return (dto, party);
    }

    public static async Task<PartySmsResultDto> SendAsync(
        PartySmsSubject subject, SendPartySmsDto request, IPartyContactDirectory parties, ISmsSender sms,
        ISmsTemplateService templates, CancellationToken ct)
    {
        if (subject.SendBlocker is not null) throw new AppException(subject.SendBlocker);
        var (preview, party) = await PreviewAsync(subject, parties, sms, templates, ct);
        if (!preview.CanSend) throw new AppException(preview.Reason ?? "This SMS cannot be sent.");

        var message = string.IsNullOrWhiteSpace(request.Message) ? preview.Message : request.Message.Trim();
        var requestId = await sms.QueueAsync([preview.Phone!], message, subject.Source,
            $"{subject.Source.ToLowerInvariant()}-sms:{request.IdempotencyKey.Trim()}", subject.Reference, ct);
        return new PartySmsResultDto { RequestId = requestId, PartyName = party!.Name, Phone = preview.Phone! };
    }

    private static PartySmsPreviewDto Blocked(PartySmsPreviewDto dto, string reason)
    {
        dto.CanSend = false;
        dto.Reason = reason;
        return dto;
    }

    // ── Payments ──

    public static async Task<PartySmsSubject> ForPaymentAsync(AppDbContext db, Guid id, CancellationToken ct)
    {
        var p = await db.Set<Models.Entities.Payment>().AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Payment), id);

        var inflow = PaymentDirection.IsInflow(p);
        PartyType? type = p.CustomerId.HasValue ? PartyType.Customer : p.VendorId.HasValue ? PartyType.Vendor : null;
        var partyId = p.CustomerId ?? p.VendorId;
        var reference = p.MoneyReceiptNumber ?? p.PaymentNumber;
        return new PartySmsSubject(
            type, partyId,
            inflow ? SmsTemplateEvents.MoneyReceived : SmsTemplateEvents.MoneyPaid,
            reference,
            Values("", p.Amount, p.PaymentDate, reference, Words(p.PaymentMethod.ToString())),
            "Payment", null);
    }

    // ── Vouchers ──

    public static async Task<PartySmsSubject> ForVoucherAsync(AppDbContext db, Guid id, CancellationToken ct)
    {
        var v = await db.Set<Models.Entities.Voucher>().AsNoTracking()
            .Include(x => x.DebitAccount).Include(x => x.CreditAccount)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id, ct)
            ?? throw new NotFoundException(nameof(Models.Entities.Voucher), id);

        // The money side names the method: a payment leaves its credit account, a receipt lands in its debit account.
        var (eventKey, method) = v.VoucherType switch
        {
            VoucherType.Credit => (SmsTemplateEvents.MoneyReceived, v.DebitAccount.AccountName),
            VoucherType.Debit => (SmsTemplateEvents.MoneyPaid, v.CreditAccount.AccountName),
            _ => ((string?)null, string.Empty),
        };
        var blocker = eventKey is null ? "A contra voucher moves money between your own accounts — there is no party to text." : null;
        var sendBlocker = v.Status != VoucherStatus.Posted ? "The SMS can only be sent once the voucher is posted." : null;

        return new PartySmsSubject(v.PartyType, v.PartyId, eventKey, v.VoucherNumber,
            Values("", v.Amount, v.VoucherDate, v.VoucherNumber, method), "Voucher", blocker, sendBlocker);
    }
}

public class GetPaymentSmsPreviewQueryHandler(AppDbContext db, IPartyContactDirectory parties, ISmsSender sms, ISmsTemplateService templates)
    : IRequestHandler<GetPaymentSmsPreviewQuery, PartySmsPreviewDto>
{
    public async Task<PartySmsPreviewDto> Handle(GetPaymentSmsPreviewQuery request, CancellationToken ct)
        => (await PartySms.PreviewAsync(await PartySms.ForPaymentAsync(db, request.PaymentId, ct), parties, sms, templates, ct)).Preview;
}

public class SendPaymentSmsCommandHandler(AppDbContext db, IPartyContactDirectory parties, ISmsSender sms, ISmsTemplateService templates)
    : IRequestHandler<SendPaymentSmsCommand, PartySmsResultDto>
{
    public async Task<PartySmsResultDto> Handle(SendPaymentSmsCommand request, CancellationToken ct)
        => await PartySms.SendAsync(await PartySms.ForPaymentAsync(db, request.PaymentId, ct), request.Dto, parties, sms, templates, ct);
}

public class GetVoucherSmsPreviewQueryHandler(AppDbContext db, IPartyContactDirectory parties, ISmsSender sms, ISmsTemplateService templates)
    : IRequestHandler<GetVoucherSmsPreviewQuery, PartySmsPreviewDto>
{
    public async Task<PartySmsPreviewDto> Handle(GetVoucherSmsPreviewQuery request, CancellationToken ct)
        => (await PartySms.PreviewAsync(await PartySms.ForVoucherAsync(db, request.VoucherId, ct), parties, sms, templates, ct)).Preview;
}

public class SendVoucherSmsCommandHandler(AppDbContext db, IPartyContactDirectory parties, ISmsSender sms, ISmsTemplateService templates)
    : IRequestHandler<SendVoucherSmsCommand, PartySmsResultDto>
{
    public async Task<PartySmsResultDto> Handle(SendVoucherSmsCommand request, CancellationToken ct)
        => await PartySms.SendAsync(await PartySms.ForVoucherAsync(db, request.VoucherId, ct), request.Dto, parties, sms, templates, ct);
}
