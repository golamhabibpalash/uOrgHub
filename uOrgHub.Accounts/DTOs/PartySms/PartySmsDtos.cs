using uOrgHub.Shared.Models.Enums;

namespace uOrgHub.Accounts.DTOs.PartySms;

/// <summary>
/// The SMS a payment or voucher would send its party: recipient from the party's own record and
/// the message from the company's template. <see cref="CanSend"/> false says why in <see cref="Reason"/>.
/// </summary>
public class PartySmsPreviewDto
{
    public bool CanSend { get; set; }
    public string? Reason { get; set; }
    public PartyType? PartyType { get; set; }
    public string? PartyName { get; set; }
    /// <summary>The number on the party's record, normalised (e.g. 8801712345678).</summary>
    public string? Phone { get; set; }
    public string? EventKey { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Reference { get; set; }
}

public class SendPartySmsDto
{
    /// <summary>The (possibly edited) text to send. Blank sends the template text.</summary>
    public string? Message { get; set; }
    /// <summary>Generated per dialog opening: a double-click sends once, a later deliberate re-send is new.</summary>
    public string IdempotencyKey { get; set; } = string.Empty;
}

public class PartySmsResultDto
{
    public Guid RequestId { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
