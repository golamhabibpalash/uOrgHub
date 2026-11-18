using uOrgHub.Sms.Providers;

namespace uOrgHub.Sms.DTOs;

public class CreateSmsGatewayDto
{
    public string Name { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string? SenderId { get; set; }
    public string? BaseUrl { get; set; }
    /// <summary>Non-secret provider settings, keyed by the descriptor's field keys.</summary>
    public Dictionary<string, string?> Settings { get; set; } = new();
    /// <summary>Secret provider settings. Write-only: never echoed back by the API.</summary>
    public Dictionary<string, string?> Credentials { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public decimal? RatePerSegment { get; set; }
    public string Currency { get; set; } = "BDT";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxSendsPerSecond { get; set; } = 5;
}

/// <summary>
/// As create, except the provider cannot change, and a credential left blank (or absent) keeps
/// its stored value — the form never sees stored secrets, so it can't send them back.
/// <see cref="ClearCredentials"/> lists the ones to remove.
/// </summary>
public class UpdateSmsGatewayDto
{
    public string Name { get; set; } = string.Empty;
    public string? SenderId { get; set; }
    public string? BaseUrl { get; set; }
    public Dictionary<string, string?> Settings { get; set; } = new();
    public Dictionary<string, string?> Credentials { get; set; } = new();
    public List<string> ClearCredentials { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public decimal? RatePerSegment { get; set; }
    public string Currency { get; set; } = "BDT";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxSendsPerSecond { get; set; } = 5;
}

public class TestSmsGatewayDto
{
    /// <summary>Optional. When given, a real test SMS is sent to this number and recorded in the history.</summary>
    public string? TestRecipient { get; set; }
}

/// <summary>A stored secret's presence — the value itself is never returned.</summary>
public record SmsCredentialStateDto(string Key, bool IsSet);

public class SmsGatewayResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? SenderId { get; set; }
    public string? BaseUrl { get; set; }
    public string? EffectiveBaseUrl { get; set; }
    public Dictionary<string, string> Settings { get; set; } = new();
    public List<SmsCredentialStateDto> Credentials { get; set; } = new();
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public decimal? RatePerSegment { get; set; }
    public decimal? LastProviderRate { get; set; }
    public decimal? EffectiveRate { get; set; }
    public string Currency { get; set; } = "BDT";
    public int TimeoutSeconds { get; set; }
    public int MaxSendsPerSecond { get; set; }
    public decimal? LastBalance { get; set; }
    public string? BalanceUnit { get; set; }
    public DateTime? LastBalanceSyncedAt { get; set; }
    public string? LastBalanceError { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public bool? LastTestSucceeded { get; set; }
    public string? LastTestMessage { get; set; }
    public SmsProviderCapabilities? Capabilities { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record SmsGatewayTestResultDto(bool Success, string Message, decimal? Balance, string? BalanceUnit, Guid? TestMessageId);

public record SmsBalanceDto(
    Guid GatewayId, string GatewayName, bool Supported, bool Success,
    decimal? Balance, string? Unit, decimal? Rate, string Currency, DateTime? SyncedAt, string? Error);
