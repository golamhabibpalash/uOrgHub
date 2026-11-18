using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;
using uOrgHub.Sms.Models.Enums;

namespace uOrgHub.Sms.Models.Entities;

/// <summary>
/// One SMS to one recipient: what was sent, through which gateway, what the gateway said, what
/// it cost. The gateway's name and provider are copied in, so history still reads correctly after
/// the gateway is renamed, reconfigured or removed.
/// </summary>
[Table("sms_messages")]
public class SmsMessage : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    public Guid? SendRequestId { get; set; }
    public SmsSendRequest? SendRequest { get; set; }

    public Guid GatewayId { get; set; }
    public SmsGateway Gateway { get; set; } = null!;

    [Required, MaxLength(100)]
    public string GatewayName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>Normalised number, digits only with country code (e.g. 8801712345678).</summary>
    [Required, MaxLength(20)]
    public string Recipient { get; set; } = string.Empty;

    [Required]
    public string MessageText { get; set; } = string.Empty;

    public SmsEncoding Encoding { get; set; }
    public int Segments { get; set; }

    public SmsMessageStatus Status { get; set; } = SmsMessageStatus.Queued;

    /// <summary>The provider's own ID for the message — the reference to quote to the vendor.</summary>
    [MaxLength(100)]
    public string? ProviderMessageId { get; set; }

    /// <summary>The provider's raw status word, kept alongside our normalised <see cref="Status"/>.</summary>
    [MaxLength(100)]
    public string? ProviderStatus { get; set; }

    [MaxLength(50)]
    public string? ErrorCode { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>The provider's last response body, truncated, with credential values redacted.</summary>
    [MaxLength(2000)]
    public string? ProviderResponse { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? Cost { get; set; }

    /// <summary>True when <see cref="Cost"/> came from the provider; false when it is segments × rate.</summary>
    public bool CostIsActual { get; set; }

    /// <summary>Send attempts made — retries only follow failures where the message certainly did not go out.</summary>
    public int Attempts { get; set; }
    public DateTime? NextAttemptAt { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }

    public int StatusChecks { get; set; }
    public DateTime? LastStatusCheckAt { get; set; }
}
