using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Sms.Models.Entities;

/// <summary>
/// One configured SMS provider account for a company. The provider's own behaviour lives in an
/// <c>ISmsProvider</c> implementation chosen by <see cref="ProviderKey"/>; this row only holds
/// that account's settings, so switching vendor is a data change, never a code change.
///
/// At most one gateway per company is <see cref="IsDefault"/> — the one every send goes through.
/// </summary>
[Table("sms_gateways")]
public class SmsGateway : BaseEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Which provider implementation drives this gateway (e.g. "bulksmsbd").</summary>
    [Required, MaxLength(50)]
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>Sender ID / masking name approved by the provider. Some providers require it.</summary>
    [MaxLength(50)]
    public string? SenderId { get; set; }

    /// <summary>API base URL; null uses the provider's default.</summary>
    [MaxLength(500)]
    public string? BaseUrl { get; set; }

    /// <summary>Non-secret, provider-specific settings as a JSON object of string values.</summary>
    public string? SettingsJson { get; set; }

    /// <summary>
    /// Secret settings (API keys, passwords, tokens), encrypted at rest by
    /// <c>ISmsCredentialProtector</c>. Never returned by the API and never logged.
    /// </summary>
    public string? CredentialsCipher { get; set; }

    /// <summary>A disabled gateway cannot be made the default and its queued messages fail.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>The company's active gateway. Unique per company (filtered index).</summary>
    public bool IsDefault { get; set; }

    /// <summary>Price of one SMS segment, entered by hand — used when the provider's API has no rate.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? RatePerSegment { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "BDT";

    /// <summary>How long to wait for the provider before giving up on one call.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Throttle so a bulk send stays inside the provider's rate limit.</summary>
    public int MaxSendsPerSecond { get; set; } = 5;

    [Column(TypeName = "decimal(18,4)")]
    public decimal? LastBalance { get; set; }

    /// <summary>What the balance is counted in ("BDT", "SMS"…), as the provider reports it.</summary>
    [MaxLength(20)]
    public string? BalanceUnit { get; set; }

    /// <summary>Per-segment rate as last reported by the provider, when it reports one.</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal? LastProviderRate { get; set; }

    public DateTime? LastBalanceSyncedAt { get; set; }

    [MaxLength(500)]
    public string? LastBalanceError { get; set; }

    public DateTime? LastTestedAt { get; set; }
    public bool? LastTestSucceeded { get; set; }

    [MaxLength(500)]
    public string? LastTestMessage { get; set; }

    /// <summary>The rate sends are costed at: the provider's own figure if it has one, else the manual rate.</summary>
    [NotMapped]
    public decimal? EffectiveRate => LastProviderRate ?? RatePerSegment;
}
