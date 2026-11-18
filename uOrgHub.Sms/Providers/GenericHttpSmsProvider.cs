using System.Text;
using System.Text.Json;
using uOrgHub.Sms.Services;

namespace uOrgHub.Sms.Providers;

/// <summary>
/// Any vendor with a plain HTTP API, configured entirely from the gateway form: the send URL,
/// method and body are templates with placeholders, and the response is read with dot paths.
/// This is how a vendor without a dedicated adapter (automas.com.bd and the like) is connected
/// without a code change.
///
/// Placeholders: {api_key} {password} {username} {sender_id} {to} {message}. Values are
/// URL-encoded in the URL and in a form body, and JSON-escaped in a JSON body.
/// </summary>
public sealed class GenericHttpSmsProvider : ISmsProvider
{
    public SmsProviderDescriptor Descriptor { get; } = new(
        "generic_http",
        "Generic HTTP API",
        "Any provider with a simple HTTP API. Describe the send request with placeholders {api_key} {password} {username} {sender_id} {to} {message}, and say where in the JSON reply to find success and the message ID.",
        DefaultBaseUrl: null,
        Fields:
        [
            new("api_key", "API key / token", IsSecret: true, Help: "Used as {api_key}."),
            new("password", "Password", IsSecret: true, Help: "Used as {password}, for providers that log in with user + password."),
            new("username", "Username", Help: "Used as {username}."),
            new("send_url", "Send URL", Required: true,
                Placeholder: "https://api.example.com/sms?key={api_key}&to={to}&msg={message}&from={sender_id}",
                Help: "Full URL. Put credentials in as placeholders, never typed in, so they stay encrypted."),
            new("send_method", "HTTP method", DefaultValue: "GET", Kind: "select", Options: ["GET", "POST"]),
            new("body_format", "Body format (POST)", DefaultValue: "form", Kind: "select", Options: ["form", "json"]),
            new("body_template", "Body template (POST)", Kind: "textarea",
                Placeholder: "api_key={api_key}\nto={to}\nmessage={message}",
                Help: "form: one name=value per line. json: a JSON object with placeholders as string values, e.g. {\"to\":\"{to}\",\"text\":\"{message}\"}."),
            new("phone_format", "Number format", DefaultValue: "8801XXXXXXXXX", Kind: "select",
                Options: ["8801XXXXXXXXX", "01XXXXXXXXX", "+8801XXXXXXXXX"]),
            new("success_path", "Success field (JSON path)", Placeholder: "status",
                Help: "Dot path to a field in the JSON reply, e.g. 'status' or 'data.code'. Empty: any HTTP 2xx counts as accepted."),
            new("success_value", "Success value", Placeholder: "success",
                Help: "Value of that field meaning accepted (case-insensitive). Empty: field must be present and not false/0."),
            new("success_contains", "Or: success text", Help: "For non-JSON replies: accepted when the reply contains this text."),
            new("message_id_path", "Message ID (JSON path)", Placeholder: "data.message_id"),
            new("error_path", "Error message (JSON path)", Placeholder: "message"),
            new("balance_url", "Balance URL", Placeholder: "https://api.example.com/balance?key={api_key}",
                Help: "Optional. GET request; leave empty if the provider has no balance API."),
            new("balance_path", "Balance (JSON path)", Placeholder: "balance"),
        ],
        new SmsProviderCapabilities(Balance: true, Rate: false, DeliveryReports: false, RequiresSenderId: false));

    public async Task<SmsSendResult> SendAsync(SmsProviderContext context, SmsOutgoingMessage message, CancellationToken ct)
    {
        var values = Values(context, FormatNumber(message.Recipient, context.Setting("phone_format", "8801XXXXXXXXX")), message.Text);
        var url = Fill(context.Setting("send_url"), values, Uri.EscapeDataString);
        if (string.IsNullOrWhiteSpace(url))
            return new SmsSendResult(SmsSendOutcome.Rejected, ErrorCode: "CONFIG", ErrorMessage: "Send URL is not configured.");

        var request = new HttpRequestMessage(
            context.Setting("send_method", "GET").Equals("POST", StringComparison.OrdinalIgnoreCase) ? HttpMethod.Post : HttpMethod.Get, url);
        if (request.Method == HttpMethod.Post)
            request.Content = BuildBody(context, values);

        SmsHttpResponse response;
        try
        {
            response = await SmsHttp.SendAsync(context, request, ct);
        }
        catch (SmsTransportException ex)
        {
            return SmsHttp.FromTransport(ex);
        }

        var raw = SmsHttp.Sanitize(response.Body, context);
        if (SmsHttp.FromHttpError(response, b => SmsHttp.Sanitize(b, context)) is { } httpError) return httpError;

        var json = SmsHttp.TryParseJson(response.Body);
        if (!IsSuccess(context, response.Body, json))
        {
            var errorPath = context.Setting("error_path");
            var error = string.IsNullOrWhiteSpace(errorPath) ? null : SmsHttp.AsText(SmsHttp.Select(json, errorPath));
            return new SmsSendResult(SmsSendOutcome.Rejected, ErrorCode: "REJECTED",
                ErrorMessage: string.IsNullOrWhiteSpace(error) ? "The gateway's reply did not indicate success." : error,
                RawResponse: raw);
        }

        var idPath = context.Setting("message_id_path");
        return new SmsSendResult(SmsSendOutcome.Accepted,
            ProviderMessageId: string.IsNullOrWhiteSpace(idPath) ? null : SmsHttp.AsText(SmsHttp.Select(json, idPath)),
            ProviderStatus: "Submitted",
            RawResponse: raw);
    }

    public async Task<SmsBalanceResult> GetBalanceAsync(SmsProviderContext context, CancellationToken ct)
    {
        var template = context.Setting("balance_url");
        if (string.IsNullOrWhiteSpace(template)) return SmsBalanceResult.NotSupported;

        try
        {
            var url = Fill(template, Values(context, "", ""), Uri.EscapeDataString);
            var response = await SmsHttp.SendAsync(context, new HttpRequestMessage(HttpMethod.Get, url), ct);
            var json = SmsHttp.TryParseJson(response.Body);
            // A plain-text reply ("1520.50") is read as the number itself.
            var balance = json is null
                ? (decimal.TryParse(response.Body.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var b) ? b : null)
                : SmsHttp.AsDecimal(SmsHttp.Select(json, context.Setting("balance_path")));
            return balance is null
                ? SmsBalanceResult.Failed($"Balance not found in the reply (HTTP {(int)response.StatusCode}). Check the balance path.")
                : new SmsBalanceResult(true, true, balance);
        }
        catch (SmsTransportException ex)
        {
            return SmsBalanceResult.Failed(ex.Message);
        }
    }

    public Task<SmsDeliveryResult> GetDeliveryStatusAsync(SmsProviderContext context, string providerMessageId, string recipient, CancellationToken ct)
        => Task.FromResult(SmsDeliveryResult.NotSupported);

    private static bool IsSuccess(SmsProviderContext context, string body, JsonElement? json)
    {
        var contains = context.Setting("success_contains");
        if (!string.IsNullOrWhiteSpace(contains))
            return body.Contains(contains, StringComparison.OrdinalIgnoreCase);

        var path = context.Setting("success_path");
        if (string.IsNullOrWhiteSpace(path)) return true; // HTTP 2xx already checked

        var actual = SmsHttp.AsText(SmsHttp.Select(json, path));
        var expected = context.Setting("success_value");
        if (actual is null) return false;
        return string.IsNullOrWhiteSpace(expected)
            ? actual is not ("false" or "0" or "")
            : actual.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }

    private static HttpContent BuildBody(SmsProviderContext context, Dictionary<string, string> values)
    {
        var template = context.Setting("body_template");
        if (context.Setting("body_format", "form").Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            // JSON-escape each value, then strip the quotes the serializer adds: the template supplies them.
            var json = Fill(template, values, v => JsonSerializer.Serialize(v)[1..^1]);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        var pairs = template
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split('=', 2))
            .Where(p => p.Length == 2)
            .Select(p => new KeyValuePair<string, string>(p[0].Trim(), Fill(p[1].Trim(), values, v => v)));
        return new FormUrlEncodedContent(pairs);
    }

    private static Dictionary<string, string> Values(SmsProviderContext context, string to, string message) => new()
    {
        ["api_key"] = context.Secret("api_key"),
        ["password"] = context.Secret("password"),
        ["username"] = context.Setting("username"),
        ["sender_id"] = context.SenderId ?? string.Empty,
        ["to"] = to,
        ["message"] = message,
    };

    public static string Fill(string template, Dictionary<string, string> values, Func<string, string> encode)
    {
        var result = template;
        foreach (var (key, value) in values)
            result = result.Replace("{" + key + "}", encode(value), StringComparison.OrdinalIgnoreCase);
        return result;
    }

    private static string FormatNumber(string normalized, string format) => format switch
    {
        "01XXXXXXXXX" => SmsPhoneNumber.ToLocal(normalized),
        "+8801XXXXXXXXX" => "+" + normalized,
        _ => normalized,
    };
}
