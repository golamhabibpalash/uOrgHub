namespace uOrgHub.Sms.Providers;

/// <summary>
/// BulkSMSBD (bulksmsbd.net). Send: POST {base}/smsapi with api_key, type, number, senderid,
/// message → JSON with <c>response_code</c> 202 on success. Balance: GET {base}/getBalanceApi.
/// No delivery-report or rate endpoint is used.
///
/// Written from the vendor's API documentation and not yet verified against a live account —
/// confirm with one before relying on it (Test
/// connection checks the key via the balance call). The base URL and the "type" value are
/// settings, so a change on the vendor's side is a configuration fix, not a code change.
/// </summary>
public sealed class BulkSmsBdProvider : ISmsProvider
{
    // Vendor response codes that mean "try again later" rather than "this message is refused".
    private static readonly HashSet<string> TransientCodes = ["1005"];

    private static readonly Dictionary<string, string> KnownErrors = new()
    {
        ["1001"] = "Invalid number.",
        ["1002"] = "Sender ID is not correct or is disabled.",
        ["1003"] = "Required fields missing — check the API key and sender ID.",
        ["1005"] = "Gateway internal error.",
        ["1006"] = "Balance validity has expired.",
        ["1007"] = "Insufficient balance.",
        ["1011"] = "User ID not found.",
        ["1012"] = "Masking SMS must be sent in Bengali.",
        ["1013"] = "Sender ID has no gateway assigned for this API key.",
        ["1018"] = "Account is disabled.",
        ["1019"] = "Account price is disabled.",
        ["1031"] = "Account is not verified.",
        ["1032"] = "Server IP is not whitelisted with the gateway.",
    };

    public SmsProviderDescriptor Descriptor { get; } = new(
        "bulksmsbd",
        "BulkSMSBD",
        "bulksmsbd.net — API key and an approved sender ID.",
        DefaultBaseUrl: "http://bulksmsbd.net/api",
        Fields:
        [
            new("api_key", "API key", IsSecret: true, Required: true, Help: "From the BulkSMSBD panel → API."),
            new("type", "Message type parameter", DefaultValue: "text",
                Help: "Value sent as 'type'. BulkSMSBD uses 'text' for both English and Bangla; change only if the vendor tells you to."),
        ],
        new SmsProviderCapabilities(Balance: true, Rate: false, DeliveryReports: false, RequiresSenderId: true));

    public async Task<SmsSendResult> SendAsync(SmsProviderContext context, SmsOutgoingMessage message, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["api_key"] = context.Secret("api_key"),
            ["type"] = context.Setting("type", "text"),
            ["number"] = message.Recipient,
            ["senderid"] = context.SenderId ?? string.Empty,
            ["message"] = message.Text,
        };

        SmsHttpResponse response;
        try
        {
            response = await SmsHttp.SendAsync(context,
                new HttpRequestMessage(HttpMethod.Post, context.Url("smsapi")) { Content = new FormUrlEncodedContent(form) }, ct);
        }
        catch (SmsTransportException ex)
        {
            return SmsHttp.FromTransport(ex);
        }

        var raw = SmsHttp.Sanitize(response.Body, context);
        if (SmsHttp.FromHttpError(response, b => SmsHttp.Sanitize(b, context)) is { } httpError) return httpError;

        var json = SmsHttp.TryParseJson(response.Body);
        var code = SmsHttp.AsText(SmsHttp.Select(json, "response_code"));
        if (code == "202")
            return new SmsSendResult(SmsSendOutcome.Accepted,
                ProviderMessageId: SmsHttp.AsText(SmsHttp.Select(json, "message_id")),
                ProviderStatus: SmsHttp.AsText(SmsHttp.Select(json, "success_message")) ?? "Submitted",
                RawResponse: raw);

        var error = SmsHttp.AsText(SmsHttp.Select(json, "error_message"));
        var message_ = string.IsNullOrWhiteSpace(error)
            ? (code is not null && KnownErrors.TryGetValue(code, out var known) ? known : "Gateway refused the message.")
            : error;
        return new SmsSendResult(
            code is not null && TransientCodes.Contains(code) ? SmsSendOutcome.Retryable : SmsSendOutcome.Rejected,
            ErrorCode: code ?? "UNKNOWN", ErrorMessage: message_, RawResponse: raw);
    }

    public async Task<SmsBalanceResult> GetBalanceAsync(SmsProviderContext context, CancellationToken ct)
    {
        try
        {
            var url = context.Url("getBalanceApi") + "?api_key=" + Uri.EscapeDataString(context.Secret("api_key"));
            var response = await SmsHttp.SendAsync(context, new HttpRequestMessage(HttpMethod.Get, url), ct);
            var json = SmsHttp.TryParseJson(response.Body);
            var balance = SmsHttp.AsDecimal(SmsHttp.Select(json, "balance"));
            if (balance is not null)
                return new SmsBalanceResult(true, true, balance, "BDT");

            var code = SmsHttp.AsText(SmsHttp.Select(json, "response_code"));
            var error = SmsHttp.AsText(SmsHttp.Select(json, "error_message"));
            return SmsBalanceResult.Failed(error
                ?? (code is not null && KnownErrors.TryGetValue(code, out var known) ? known : $"Unexpected balance response (HTTP {(int)response.StatusCode})."));
        }
        catch (SmsTransportException ex)
        {
            return SmsBalanceResult.Failed(ex.Message);
        }
    }

    public Task<SmsDeliveryResult> GetDeliveryStatusAsync(SmsProviderContext context, string providerMessageId, string recipient, CancellationToken ct)
        => Task.FromResult(SmsDeliveryResult.NotSupported);
}
