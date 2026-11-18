using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;
using uOrgHub.Sms.Models.Enums;

namespace uOrgHub.Sms.Models.Entities;

/// <summary>
/// One "Send" — a message to one or many recipients — and the idempotency key that makes a
/// double-click or a retried request return this same send instead of a second one.
/// Each recipient gets its own <see cref="SmsMessage"/>.
/// </summary>
[Table("sms_send_requests")]
public class SmsSendRequest : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    /// <summary>Client-generated, unique per company.</summary>
    [Required, MaxLength(100)]
    public string IdempotencyKey { get; set; } = string.Empty;

    public Guid GatewayId { get; set; }
    public SmsGateway Gateway { get; set; } = null!;

    [Required]
    public string MessageText { get; set; } = string.Empty;

    public SmsEncoding Encoding { get; set; }
    public int SegmentsPerRecipient { get; set; }
    public int RecipientCount { get; set; }
    public int TotalSegments { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? RatePerSegment { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? EstimatedCost { get; set; }

    public Guid? RequestedByUserId { get; set; }

    /// <summary>What raised the send: "Manual" from the compose page, "GatewayTest", or a module name.</summary>
    [Required, MaxLength(50)]
    public string Source { get; set; } = "Manual";

    /// <summary>The document behind the send (MR No., payment or voucher number), when it came from one.</summary>
    [MaxLength(100)]
    public string? SourceReference { get; set; }

    public ICollection<SmsMessage> Messages { get; set; } = new List<SmsMessage>();
}
