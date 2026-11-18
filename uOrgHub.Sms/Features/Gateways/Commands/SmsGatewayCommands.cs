using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Sms.DTOs;
using uOrgHub.Sms.Features._Common;
using uOrgHub.Sms.Models.Entities;
using uOrgHub.Sms.Providers;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.Features.Gateways.Commands;

public record CreateSmsGatewayCommand(CreateSmsGatewayDto Dto) : ICommand<SmsGatewayResponseDto>;
public record UpdateSmsGatewayCommand(Guid Id, UpdateSmsGatewayDto Dto) : ICommand<SmsGatewayResponseDto>;
public record DeleteSmsGatewayCommand(Guid Id) : ICommand<Unit>;
public record SetSmsGatewayEnabledCommand(Guid Id, bool Enabled) : ICommand<SmsGatewayResponseDto>;
public record ActivateSmsGatewayCommand(Guid Id) : ICommand<SmsGatewayResponseDto>;
public record TestSmsGatewayCommand(Guid Id, TestSmsGatewayDto Dto, Guid? UserId) : ICommand<SmsGatewayTestResultDto>;
/// <summary>Refreshes the balance from the provider. Null gateway = the active one. Throttled unless <paramref name="Force"/>.</summary>
public record SyncSmsBalanceCommand(Guid? GatewayId, bool Force = false) : ICommand<SmsBalanceDto?>;

/// <summary>
/// Settings and credentials checked against the provider's descriptor: unknown keys dropped,
/// required fields enforced, secrets kept apart from plain settings.
/// </summary>
internal static class GatewayConfig
{
    public static void Apply(
        SmsGateway gateway, SmsProviderDescriptor descriptor, ISmsCredentialProtector protector,
        Dictionary<string, string?> settings, Dictionary<string, string?> credentials, IEnumerable<string> clear)
    {
        var plain = descriptor.Fields.Where(f => !f.IsSecret)
            .Select(f => (f.Key, Value: settings.GetValueOrDefault(f.Key)?.Trim()))
            .Where(x => !string.IsNullOrEmpty(x.Value))
            .ToDictionary(x => x.Key, x => x.Value!);

        var secrets = protector.Unprotect(gateway.CredentialsCipher);
        foreach (var key in clear) secrets.Remove(key);
        foreach (var field in descriptor.Fields.Where(f => f.IsSecret))
        {
            var value = credentials.GetValueOrDefault(field.Key)?.Trim();
            if (!string.IsNullOrEmpty(value)) secrets[field.Key] = value; // blank = keep stored value
        }
        secrets = secrets.Where(kv => descriptor.Fields.Any(f => f.IsSecret && f.Key == kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        var errors = new List<string>();
        foreach (var field in descriptor.Fields.Where(f => f.Required))
        {
            var present = field.IsSecret ? secrets.ContainsKey(field.Key) : plain.ContainsKey(field.Key) || field.DefaultValue is not null;
            if (!present) errors.Add($"{field.Label} is required for {descriptor.DisplayName}.");
        }
        if (descriptor.Capabilities.RequiresSenderId && string.IsNullOrWhiteSpace(gateway.SenderId))
            errors.Add($"Sender ID is required for {descriptor.DisplayName}.");
        if (string.IsNullOrWhiteSpace(gateway.BaseUrl) && string.IsNullOrWhiteSpace(descriptor.DefaultBaseUrl)
            && !descriptor.Fields.Any(f => f.Key == "send_url"))
            errors.Add("Base URL is required for this provider.");
        if (errors.Count > 0) throw new ValidationException(errors);

        gateway.SettingsJson = plain.Count == 0 ? null : JsonSerializer.Serialize(plain);
        gateway.CredentialsCipher = protector.Protect(secrets);
    }

    public static async Task EnsureUniqueNameAsync(AppDbContext db, string name, Guid? exceptId, CancellationToken ct)
    {
        var trimmed = name.Trim();
        if (await db.Set<SmsGateway>().AnyAsync(g => !g.IsDeleted && g.Name == trimmed && g.Id != exceptId, ct))
            throw new AppException($"An SMS gateway named '{trimmed}' already exists.");
    }

    public static async Task<SmsGateway> LoadAsync(AppDbContext db, Guid id, CancellationToken ct)
        => await db.Set<SmsGateway>().FirstOrDefaultAsync(g => !g.IsDeleted && g.Id == id, ct)
           ?? throw new NotFoundException(nameof(SmsGateway), id);
}

public class CreateSmsGatewayCommandHandler(
    AppDbContext db, ISmsProviderRegistry registry, ISmsCredentialProtector protector, ISmsGatewayRuntime runtime,
    ILogger<CreateSmsGatewayCommandHandler> logger)
    : IRequestHandler<CreateSmsGatewayCommand, SmsGatewayResponseDto>
{
    public async Task<SmsGatewayResponseDto> Handle(CreateSmsGatewayCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        if (!registry.Exists(dto.ProviderKey))
            throw new ValidationException([$"Unknown SMS provider '{dto.ProviderKey}'."]);
        await GatewayConfig.EnsureUniqueNameAsync(db, dto.Name, null, ct);

        var gateway = new SmsGateway
        {
            Name = dto.Name.Trim(),
            ProviderKey = registry.Get(dto.ProviderKey).Descriptor.Key,
            SenderId = string.IsNullOrWhiteSpace(dto.SenderId) ? null : dto.SenderId.Trim(),
            BaseUrl = string.IsNullOrWhiteSpace(dto.BaseUrl) ? null : dto.BaseUrl.Trim(),
            IsEnabled = dto.IsEnabled,
            RatePerSegment = dto.RatePerSegment,
            Currency = dto.Currency.Trim().ToUpperInvariant(),
            TimeoutSeconds = dto.TimeoutSeconds,
            MaxSendsPerSecond = dto.MaxSendsPerSecond,
        };
        GatewayConfig.Apply(gateway, registry.Get(gateway.ProviderKey).Descriptor, protector, dto.Settings, dto.Credentials, []);

        // The company's first enabled gateway becomes the active one, so sending works straight away.
        gateway.IsDefault = gateway.IsEnabled && !await db.Set<SmsGateway>().AnyAsync(g => !g.IsDeleted && g.IsDefault, ct);

        db.Set<SmsGateway>().Add(gateway);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SMS gateway {GatewayId} '{Name}' ({Provider}) created", gateway.Id, gateway.Name, gateway.ProviderKey);
        return runtime.ToDto(gateway);
    }
}

public class UpdateSmsGatewayCommandHandler(
    AppDbContext db, ISmsProviderRegistry registry, ISmsCredentialProtector protector, ISmsGatewayRuntime runtime,
    ILogger<UpdateSmsGatewayCommandHandler> logger)
    : IRequestHandler<UpdateSmsGatewayCommand, SmsGatewayResponseDto>
{
    public async Task<SmsGatewayResponseDto> Handle(UpdateSmsGatewayCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var gateway = await GatewayConfig.LoadAsync(db, request.Id, ct);
        await GatewayConfig.EnsureUniqueNameAsync(db, dto.Name, gateway.Id, ct);
        if (gateway.IsDefault && !dto.IsEnabled)
            throw new AppException("The active gateway cannot be disabled. Make another gateway active first.");

        gateway.Name = dto.Name.Trim();
        gateway.SenderId = string.IsNullOrWhiteSpace(dto.SenderId) ? null : dto.SenderId.Trim();
        gateway.BaseUrl = string.IsNullOrWhiteSpace(dto.BaseUrl) ? null : dto.BaseUrl.Trim();
        gateway.IsEnabled = dto.IsEnabled;
        gateway.RatePerSegment = dto.RatePerSegment;
        gateway.Currency = dto.Currency.Trim().ToUpperInvariant();
        gateway.TimeoutSeconds = dto.TimeoutSeconds;
        gateway.MaxSendsPerSecond = dto.MaxSendsPerSecond;
        GatewayConfig.Apply(gateway, registry.Get(gateway.ProviderKey).Descriptor, protector, dto.Settings, dto.Credentials, dto.ClearCredentials);

        await db.SaveChangesAsync(ct);
        logger.LogInformation("SMS gateway {GatewayId} '{Name}' updated (credentials changed: {Changed})",
            gateway.Id, gateway.Name, dto.Credentials.Any(c => !string.IsNullOrWhiteSpace(c.Value)) || dto.ClearCredentials.Count > 0);
        return runtime.ToDto(gateway);
    }
}

public class DeleteSmsGatewayCommandHandler(AppDbContext db, ILogger<DeleteSmsGatewayCommandHandler> logger)
    : IRequestHandler<DeleteSmsGatewayCommand, Unit>
{
    public async Task<Unit> Handle(DeleteSmsGatewayCommand request, CancellationToken ct)
    {
        var gateway = await GatewayConfig.LoadAsync(db, request.Id, ct);
        if (gateway.IsDefault)
            throw new AppException("The active gateway cannot be removed. Make another gateway active first.");

        gateway.IsDeleted = true;
        gateway.DeletedAt = DateTime.UtcNow;
        gateway.IsEnabled = false;
        // Credentials serve no purpose once removed; history keeps the gateway's name.
        gateway.CredentialsCipher = null;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SMS gateway {GatewayId} '{Name}' removed", gateway.Id, gateway.Name);
        return Unit.Value;
    }
}

public class SetSmsGatewayEnabledCommandHandler(AppDbContext db, ISmsGatewayRuntime runtime)
    : IRequestHandler<SetSmsGatewayEnabledCommand, SmsGatewayResponseDto>
{
    public async Task<SmsGatewayResponseDto> Handle(SetSmsGatewayEnabledCommand request, CancellationToken ct)
    {
        var gateway = await GatewayConfig.LoadAsync(db, request.Id, ct);
        if (!request.Enabled && gateway.IsDefault)
            throw new AppException("The active gateway cannot be disabled. Make another gateway active first.");
        gateway.IsEnabled = request.Enabled;
        await db.SaveChangesAsync(ct);
        return runtime.ToDto(gateway);
    }
}

/// <summary>
/// Makes a gateway the company's active one. Two saves, because the unique "one default per
/// company" index would reject an update that sets the new default before clearing the old one.
/// </summary>
public class ActivateSmsGatewayCommandHandler(AppDbContext db, ISmsGatewayRuntime runtime, ILogger<ActivateSmsGatewayCommandHandler> logger)
    : IRequestHandler<ActivateSmsGatewayCommand, SmsGatewayResponseDto>
{
    public async Task<SmsGatewayResponseDto> Handle(ActivateSmsGatewayCommand request, CancellationToken ct)
    {
        var gateway = await GatewayConfig.LoadAsync(db, request.Id, ct);
        if (!gateway.IsEnabled)
            throw new AppException("Enable the gateway before making it active.");
        if (gateway.IsDefault) return runtime.ToDto(gateway);

        IDbContextTransaction? tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
        await using (tx)
        {
            var previous = await db.Set<SmsGateway>().Where(g => !g.IsDeleted && g.IsDefault).ToListAsync(ct);
            foreach (var g in previous) g.IsDefault = false;
            await db.SaveChangesAsync(ct);

            gateway.IsDefault = true;
            await db.SaveChangesAsync(ct);
            if (tx is not null) await tx.CommitAsync(ct);

            logger.LogInformation("Active SMS gateway switched from {Previous} to {GatewayId} '{Name}'",
                string.Join(",", previous.Select(p => p.Id)), gateway.Id, gateway.Name);
        }
        return runtime.ToDto(gateway);
    }
}

/// <summary>
/// Checks a gateway's settings against the vendor. Uses the balance call where the provider has
/// one — it proves the URL and credentials without spending anything. With a test number, also
/// sends a real SMS through the normal queue so the result appears in history.
/// </summary>
public class TestSmsGatewayCommandHandler(
    AppDbContext db, ISmsGatewayRuntime runtime, SmsSendService sendService)
    : IRequestHandler<TestSmsGatewayCommand, SmsGatewayTestResultDto>
{
    public async Task<SmsGatewayTestResultDto> Handle(TestSmsGatewayCommand request, CancellationToken ct)
    {
        var gateway = await GatewayConfig.LoadAsync(db, request.Id, ct);
        var provider = runtime.Provider(gateway);

        bool success;
        string message;
        decimal? balance = null;
        string? unit = null;
        try
        {
            var context = runtime.CreateContext(gateway);
            var result = SmsResultRedaction.Redact(await provider.GetBalanceAsync(context, ct), context);
            if (!result.Supported)
            {
                success = true;
                message = "Settings are complete. This provider has no balance API, so credentials can only be proven by sending a test SMS.";
            }
            else if (result.Success)
            {
                success = true;
                balance = result.Balance;
                unit = result.Unit;
                message = balance is null ? "Connected." : $"Connected. Balance: {balance:0.##} {unit}".TrimEnd();
                BalanceUpdate.Apply(gateway, result);
            }
            else
            {
                success = false;
                message = result.Error ?? "The provider rejected the request.";
            }
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            success = false;
            message = "Stored credentials cannot be decrypted (the encryption key changed). Re-enter them.";
        }

        Guid? testMessageId = null;
        if (success && !string.IsNullOrWhiteSpace(request.Dto.TestRecipient))
        {
            if (!gateway.IsEnabled)
                throw new AppException("Enable the gateway to send a test SMS.");
            var (send, _) = await sendService.CreateAsync(
                [request.Dto.TestRecipient], $"uOrgHub test message from gateway '{gateway.Name}'.",
                $"gateway-test:{gateway.Id}:{Guid.NewGuid():N}", "GatewayTest", request.UserId, gateway.Id, ct);
            testMessageId = send.Messages.FirstOrDefault()?.Id;
            message += " Test SMS queued — check SMS History for the result.";
        }

        gateway.LastTestedAt = DateTime.UtcNow;
        gateway.LastTestSucceeded = success;
        gateway.LastTestMessage = message.Length > 500 ? message[..500] : message;
        await db.SaveChangesAsync(ct);

        return new SmsGatewayTestResultDto(success, message, balance, unit, testMessageId);
    }
}

internal static class BalanceUpdate
{
    public static void Apply(SmsGateway gateway, SmsBalanceResult result)
    {
        gateway.LastBalanceSyncedAt = DateTime.UtcNow;
        if (result.Success)
        {
            gateway.LastBalance = result.Balance;
            gateway.BalanceUnit = result.Unit ?? gateway.BalanceUnit;
            if (result.RatePerSegment is not null) gateway.LastProviderRate = result.RatePerSegment;
            gateway.LastBalanceError = null;
        }
        else
        {
            var error = result.Error ?? "Balance unavailable.";
            gateway.LastBalanceError = error.Length > 500 ? error[..500] : error;
        }
    }
}

public class SyncSmsBalanceCommandHandler(AppDbContext db, ISmsGatewayRuntime runtime)
    : IRequestHandler<SyncSmsBalanceCommand, SmsBalanceDto?>
{
    /// <summary>Dashboard loads refresh at most this often; the Refresh button forces it.</summary>
    private static readonly TimeSpan MinInterval = TimeSpan.FromMinutes(2);

    public async Task<SmsBalanceDto?> Handle(SyncSmsBalanceCommand request, CancellationToken ct)
    {
        var gateway = request.GatewayId is { } id
            ? await GatewayConfig.LoadAsync(db, id, ct)
            : await db.Set<SmsGateway>().FirstOrDefaultAsync(g => !g.IsDeleted && g.IsDefault, ct);
        if (gateway is null) return null;

        var provider = runtime.Provider(gateway);
        var supported = provider.Descriptor.Capabilities.Balance;
        var fresh = gateway.LastBalanceSyncedAt is { } at && DateTime.UtcNow - at < MinInterval;

        if (supported && (request.Force || !fresh))
        {
            SmsBalanceResult result;
            try
            {
                var context = runtime.CreateContext(gateway);
                result = SmsResultRedaction.Redact(await provider.GetBalanceAsync(context, ct), context);
            }
            catch (System.Security.Cryptography.CryptographicException) { result = SmsBalanceResult.Failed("Stored credentials cannot be decrypted. Re-enter them."); }

            supported = result.Supported;
            if (result.Supported)
            {
                BalanceUpdate.Apply(gateway, result);
                await db.SaveChangesAsync(ct);
            }
        }

        return new SmsBalanceDto(gateway.Id, gateway.Name, supported, supported && gateway.LastBalanceError is null,
            gateway.LastBalance, gateway.BalanceUnit, gateway.EffectiveRate, gateway.Currency,
            gateway.LastBalanceSyncedAt, gateway.LastBalanceError);
    }
}
