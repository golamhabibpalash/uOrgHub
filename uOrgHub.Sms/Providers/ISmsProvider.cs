using uOrgHub.Sms.Models.Enums;

namespace uOrgHub.Sms.Providers;

/// <summary>
/// One SMS vendor's API. Everything vendor-specific — URLs, parameter names, response formats,
/// error codes — lives behind this interface; the rest of the module only ever sees the normalised
/// results below. Adding a vendor means one new class registered in <c>SmsServiceExtension</c>:
/// no entity, migration, endpoint or screen changes (the gateway form is built from
/// <see cref="Descriptor"/>).
///
/// Implementations must be stateless (they are singletons, shared by every company) and must not
/// log or return credential values.
/// </summary>
public interface ISmsProvider
{
    SmsProviderDescriptor Descriptor { get; }

    Task<SmsSendResult> SendAsync(SmsProviderContext context, SmsOutgoingMessage message, CancellationToken ct);

    /// <summary>Remaining balance and, when the API exposes it, the per-SMS rate. Return <see cref="SmsBalanceResult.NotSupported"/> otherwise.</summary>
    Task<SmsBalanceResult> GetBalanceAsync(SmsProviderContext context, CancellationToken ct);

    /// <summary>Delivery report for one message. Return <see cref="SmsDeliveryResult.NotSupported"/> when the vendor has none.</summary>
    Task<SmsDeliveryResult> GetDeliveryStatusAsync(SmsProviderContext context, string providerMessageId, string recipient, CancellationToken ct);
}

/// <summary>What a provider is and what it needs — drives the gateway form and capability checks.</summary>
public record SmsProviderDescriptor(
    string Key,
    string DisplayName,
    string Description,
    string? DefaultBaseUrl,
    IReadOnlyList<SmsProviderField> Fields,
    SmsProviderCapabilities Capabilities);

/// <summary>
/// One provider setting. Secret fields are encrypted at rest and write-only through the API;
/// the rest are stored as plain settings.
/// </summary>
public record SmsProviderField(
    string Key,
    string Label,
    bool IsSecret = false,
    bool Required = false,
    string? Placeholder = null,
    string? Help = null,
    string? DefaultValue = null,
    string Kind = "text",
    IReadOnlyList<string>? Options = null);

public record SmsProviderCapabilities(
    bool Balance,
    bool Rate,
    bool DeliveryReports,
    bool RequiresSenderId);

/// <summary>A gateway's configuration, resolved and decrypted for one provider call.</summary>
public sealed class SmsProviderContext
{
    public required string GatewayName { get; init; }
    public required string BaseUrl { get; init; }
    public string? SenderId { get; init; }
    public required IReadOnlyDictionary<string, string> Secrets { get; init; }
    public required IReadOnlyDictionary<string, string> Settings { get; init; }
    public required HttpClient Http { get; init; }
    public required TimeSpan Timeout { get; init; }

    public string Secret(string key) => Secrets.TryGetValue(key, out var v) ? v : string.Empty;

    public string Setting(string key, string fallback = "")
        => Settings.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : fallback;

    /// <summary>Base URL joined with a path, without doubling or dropping the slash.</summary>
    public string Url(string path) => BaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
}

public record SmsOutgoingMessage(string Recipient, string Text, SmsEncoding Encoding, int Segments);

/// <summary>How a send attempt ended, from the point of view of "did it go out?".</summary>
public enum SmsSendOutcome
{
    /// <summary>The gateway accepted the message.</summary>
    Accepted,
    /// <summary>The gateway refused it (bad number, no balance, bad credentials…). Retrying won't help.</summary>
    Rejected,
    /// <summary>Certainly not sent — connection refused, rate limited, gateway unavailable — so safe to retry.</summary>
    Retryable,
    /// <summary>Request went out but no usable answer came back (timeout). May have been sent: never retried.</summary>
    Undetermined,
}

public record SmsSendResult(
    SmsSendOutcome Outcome,
    string? ProviderMessageId = null,
    string? ProviderStatus = null,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    decimal? Cost = null,
    string? RawResponse = null,
    TimeSpan? RetryAfter = null);

public record SmsBalanceResult(
    bool Supported,
    bool Success,
    decimal? Balance = null,
    string? Unit = null,
    decimal? RatePerSegment = null,
    string? Error = null)
{
    public static SmsBalanceResult NotSupported { get; } = new(false, false);
    public static SmsBalanceResult Failed(string error) => new(true, false, Error: error);
}

public record SmsDeliveryResult(
    bool Supported,
    SmsMessageStatus? Status = null,
    string? ProviderStatus = null,
    decimal? Cost = null,
    string? Error = null)
{
    public static SmsDeliveryResult NotSupported { get; } = new(false);
}
