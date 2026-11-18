using System.Text.Json;
using uOrgHub.Sms.Models.Enums;

namespace uOrgHub.Sms.Providers;

/// <summary>
/// Alpha SMS (sms.net.bd). Send: POST {base}/sendsms with api_key, msg, to[, sender_id] →
/// <c>{"error":0,"data":{"request_id":…}}</c>. Balance: GET {base}/user/balance/. Delivery report:
/// GET {base}/report/request/{request_id}/ with a per-recipient status and charge.
///
/// Written from the vendor's API documentation and not yet verified against a live account —
/// confirm with one before relying on it (Test
/// connection checks the key via the balance call).
/// </summary>
public sealed class AlphaSmsProvider : ISmsProvider
{
    public SmsProviderDescriptor Descriptor { get; } = new(
        "alphasms",
        "Alpha SMS (sms.net.bd)",
        "sms.net.bd — API key; sender ID optional (needed for masked/branded SMS).",
        DefaultBaseUrl: "https://api.sms.net.bd",
        Fields:
        [
            new("api_key", "API key", IsSecret: true, Required: true, Help: "From the Alpha SMS portal → API."),
        ],
        new SmsProviderCapabilities(Balance: true, Rate: false, DeliveryReports: true, RequiresSenderId: false));

    public async Task<SmsSendResult> SendAsync(SmsProviderContext context, SmsOutgoingMessage message, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["api_key"] = context.Secret("api_key"),
            ["msg"] = message.Text,
            ["to"] = message.Recipient,
        };
        if (!string.IsNullOrWhiteSpace(context.SenderId)) form["sender_id"] = context.SenderId;

        SmsHttpResponse response;
        try
        {
            response = await SmsHttp.SendAsync(context,
                new HttpRequestMessage(HttpMethod.Post, context.Url("sendsms")) { Content = new FormUrlEncodedContent(form) }, ct);
        }
        catch (SmsTransportException ex)
        {
            return SmsHttp.FromTransport(ex);
        }

        var raw = SmsHttp.Sanitize(response.Body, context);
        if (SmsHttp.FromHttpError(response, b => SmsHttp.Sanitize(b, context)) is { } httpError) return httpError;

        var json = SmsHttp.TryParseJson(response.Body);
        var error = SmsHttp.AsText(SmsHttp.Select(json, "error"));
        var msg = SmsHttp.AsText(SmsHttp.Select(json, "msg"));
        if (error == "0")
            return new SmsSendResult(SmsSendOutcome.Accepted,
                ProviderMessageId: SmsHttp.AsText(SmsHttp.Select(json, "data.request_id")),
                ProviderStatus: msg ?? "Submitted",
                RawResponse: raw);

        return new SmsSendResult(SmsSendOutcome.Rejected, ErrorCode: error ?? "UNKNOWN",
            ErrorMessage: msg ?? "Gateway refused the message.", RawResponse: raw);
    }

    public async Task<SmsBalanceResult> GetBalanceAsync(SmsProviderContext context, CancellationToken ct)
    {
        try
        {
            var url = context.Url("user/balance/") + "?api_key=" + Uri.EscapeDataString(context.Secret("api_key"));
            var response = await SmsHttp.SendAsync(context, new HttpRequestMessage(HttpMethod.Get, url), ct);
            var json = SmsHttp.TryParseJson(response.Body);
            if (SmsHttp.AsText(SmsHttp.Select(json, "error")) == "0")
                return new SmsBalanceResult(true, true, SmsHttp.AsDecimal(SmsHttp.Select(json, "data.balance")), "BDT");

            return SmsBalanceResult.Failed(SmsHttp.AsText(SmsHttp.Select(json, "msg"))
                ?? $"Unexpected balance response (HTTP {(int)response.StatusCode}).");
        }
        catch (SmsTransportException ex)
        {
            return SmsBalanceResult.Failed(ex.Message);
        }
    }

    public async Task<SmsDeliveryResult> GetDeliveryStatusAsync(SmsProviderContext context, string providerMessageId, string recipient, CancellationToken ct)
    {
        try
        {
            var url = context.Url($"report/request/{Uri.EscapeDataString(providerMessageId)}/") + "?api_key=" + Uri.EscapeDataString(context.Secret("api_key"));
            var response = await SmsHttp.SendAsync(context, new HttpRequestMessage(HttpMethod.Get, url), ct);
            var json = SmsHttp.TryParseJson(response.Body);
            if (SmsHttp.AsText(SmsHttp.Select(json, "error")) != "0")
                return new SmsDeliveryResult(true, Error: SmsHttp.AsText(SmsHttp.Select(json, "msg")) ?? "Report unavailable.");

            // One request can carry several recipients; ours is one, but match the number anyway.
            var recipients = SmsHttp.Select(json, "data.recipients");
            JsonElement? entry = null;
            if (recipients?.ValueKind == JsonValueKind.Array)
                foreach (var r in recipients.Value.EnumerateArray())
                    if ((SmsHttp.AsText(SmsHttp.Select(r, "number")) ?? "").EndsWith(recipient.Length > 10 ? recipient[^10..] : recipient))
                    {
                        entry = r;
                        break;
                    }

            var status = SmsHttp.AsText(SmsHttp.Select(entry, "status")) ?? SmsHttp.AsText(SmsHttp.Select(json, "data.request_status"));
            var charge = SmsHttp.AsDecimal(SmsHttp.Select(entry, "charge")) ?? SmsHttp.AsDecimal(SmsHttp.Select(json, "data.request_charge"));
            return new SmsDeliveryResult(true, Map(status), status, charge);
        }
        catch (SmsTransportException ex)
        {
            return new SmsDeliveryResult(true, Error: ex.Message);
        }
    }

    /// <summary>Vendor status word → our status; null keeps the message Submitted and checks again later.</summary>
    private static SmsMessageStatus? Map(string? status) => status?.Trim().ToLowerInvariant() switch
    {
        "sent" or "delivered" or "complete" or "completed" or "success" => SmsMessageStatus.Delivered,
        "failed" or "undelivered" or "rejected" or "expired" or "error" => SmsMessageStatus.Failed,
        _ => null,
    };
}
