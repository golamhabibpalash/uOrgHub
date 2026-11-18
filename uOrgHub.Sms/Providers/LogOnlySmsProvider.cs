using Microsoft.Extensions.Logging;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.Providers;

/// <summary>
/// Sends nothing: accepts every message and logs a masked recipient. For trying the module,
/// training and test environments without spending SMS credit.
/// </summary>
public sealed class LogOnlySmsProvider(ILogger<LogOnlySmsProvider> logger) : ISmsProvider
{
    public SmsProviderDescriptor Descriptor { get; } = new(
        "log_only",
        "Log only (test)",
        "Does not send anything. Every message is marked submitted and written to the server log with the number masked. Use it to try the module without spending SMS credit.",
        DefaultBaseUrl: "about:log",
        Fields: [],
        new SmsProviderCapabilities(Balance: false, Rate: false, DeliveryReports: false, RequiresSenderId: false));

    public Task<SmsSendResult> SendAsync(SmsProviderContext context, SmsOutgoingMessage message, CancellationToken ct)
    {
        logger.LogInformation("[SMS log-only] {Gateway}: {Segments} segment(s) to {Recipient}",
            context.GatewayName, message.Segments, SmsPhoneNumber.Mask(message.Recipient));
        return Task.FromResult(new SmsSendResult(SmsSendOutcome.Accepted,
            ProviderMessageId: "LOG-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            ProviderStatus: "Logged"));
    }

    public Task<SmsBalanceResult> GetBalanceAsync(SmsProviderContext context, CancellationToken ct)
        => Task.FromResult(SmsBalanceResult.NotSupported);

    public Task<SmsDeliveryResult> GetDeliveryStatusAsync(SmsProviderContext context, string providerMessageId, string recipient, CancellationToken ct)
        => Task.FromResult(SmsDeliveryResult.NotSupported);
}
