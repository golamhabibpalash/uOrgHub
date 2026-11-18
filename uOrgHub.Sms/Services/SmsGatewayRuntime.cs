using System.Text.Json;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Mappings;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Providers;

namespace uOrgHub.Sms.Services;

/// <summary>Turns a stored <see cref="SmsGateway"/> into something a provider can call, and into its API shape.</summary>
public interface ISmsGatewayRuntime
{
    ISmsProvider Provider(SmsGateway gateway);
    SmsProviderContext CreateContext(SmsGateway gateway);
    Dictionary<string, string> ReadSettings(SmsGateway gateway);
    SmsGatewayResponseDto ToDto(SmsGateway gateway);
}

public sealed class SmsGatewayRuntime(
    ISmsProviderRegistry registry,
    ISmsCredentialProtector protector,
    IHttpClientFactory httpFactory) : ISmsGatewayRuntime
{
    public const string HttpClientName = "sms";
    private static readonly SmsMapper Mapper = new();

    public ISmsProvider Provider(SmsGateway gateway) => registry.Get(gateway.ProviderKey);

    public SmsProviderContext CreateContext(SmsGateway gateway)
    {
        var descriptor = Provider(gateway).Descriptor;
        var settings = ReadSettings(gateway);
        // Unset settings fall back to the field's default, so the provider sees one complete set.
        foreach (var field in descriptor.Fields.Where(f => !f.IsSecret && f.DefaultValue is not null))
            settings.TryAdd(field.Key, field.DefaultValue!);

        return new SmsProviderContext
        {
            GatewayName = gateway.Name,
            BaseUrl = string.IsNullOrWhiteSpace(gateway.BaseUrl) ? descriptor.DefaultBaseUrl ?? string.Empty : gateway.BaseUrl,
            SenderId = gateway.SenderId,
            Secrets = protector.Unprotect(gateway.CredentialsCipher),
            Settings = settings,
            Http = httpFactory.CreateClient(HttpClientName),
            Timeout = TimeSpan.FromSeconds(Math.Clamp(gateway.TimeoutSeconds, 5, 120)),
        };
    }

    public Dictionary<string, string> ReadSettings(SmsGateway gateway)
        => string.IsNullOrWhiteSpace(gateway.SettingsJson)
            ? new()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(gateway.SettingsJson) ?? new();

    public SmsGatewayResponseDto ToDto(SmsGateway gateway)
    {
        var dto = Mapper.ToDto(gateway);
        dto.EffectiveRate = gateway.EffectiveRate;
        dto.Settings = ReadSettings(gateway);

        if (!registry.Exists(gateway.ProviderKey))
        {
            dto.ProviderName = gateway.ProviderKey + " (not installed)";
            return dto;
        }

        var descriptor = registry.Get(gateway.ProviderKey).Descriptor;
        dto.ProviderName = descriptor.DisplayName;
        dto.Capabilities = descriptor.Capabilities;
        dto.EffectiveBaseUrl = string.IsNullOrWhiteSpace(gateway.BaseUrl) ? descriptor.DefaultBaseUrl : gateway.BaseUrl;

        // Which secrets are stored — never what they are. A cipher that no longer decrypts (key
        // changed) reports every secret as unset, which is what the admin needs to know.
        HashSet<string> stored;
        try { stored = protector.Unprotect(gateway.CredentialsCipher).Where(kv => kv.Value.Length > 0).Select(kv => kv.Key).ToHashSet(); }
        catch (Exception) { stored = []; }
        dto.Credentials = descriptor.Fields.Where(f => f.IsSecret)
            .Select(f => new SmsCredentialStateDto(f.Key, stored.Contains(f.Key)))
            .ToList();
        return dto;
    }
}
