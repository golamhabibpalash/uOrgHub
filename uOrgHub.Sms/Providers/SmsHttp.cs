using System.Net;
using System.Text.Json;

namespace uOrgHub.Sms.Providers;

/// <summary>A provider call's raw outcome, before the provider interprets the body.</summary>
public sealed record SmsHttpResponse(HttpStatusCode StatusCode, string Body, TimeSpan? RetryAfter);

/// <summary>Thrown for a call that produced no response, classified by whether the request could have been processed.</summary>
public sealed class SmsTransportException(SmsSendOutcome outcome, string message) : Exception(message)
{
    public SmsSendOutcome Outcome { get; } = outcome;
}

/// <summary>
/// Shared HTTP plumbing for providers: per-gateway timeout, and the one distinction that matters
/// for sending — a failure where the request certainly never reached the vendor (safe to retry)
/// versus one where it may have been processed (must not be retried).
/// </summary>
public static class SmsHttp
{
    public const int MaxStoredResponse = 2000;

    public static async Task<SmsHttpResponse> SendAsync(SmsProviderContext context, HttpRequestMessage request, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(context.Timeout);
        try
        {
            using var response = await context.Http.SendAsync(request, timeout.Token);
            var body = await response.Content.ReadAsStringAsync(timeout.Token);
            var retryAfter = response.Headers.RetryAfter?.Delta;
            return new SmsHttpResponse(response.StatusCode, body, retryAfter);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new SmsTransportException(SmsSendOutcome.Undetermined,
                $"{context.GatewayName} did not respond within {context.Timeout.TotalSeconds:0} seconds.");
        }
        catch (HttpRequestException ex)
        {
            // No HTTP response at all: DNS failure, connection refused, TLS failure — the vendor
            // never saw a complete request. Message kept generic: ex.Message can quote the URL,
            // and some vendors put the API key in the query string.
            throw new SmsTransportException(SmsSendOutcome.Retryable,
                $"Could not reach {context.GatewayName} ({ex.HttpRequestError}).");
        }
    }

    /// <summary>
    /// Classifies a non-success HTTP status. 429 and 502/503/504 mean the vendor did not process
    /// the request; any other error status is treated as a rejection.
    /// </summary>
    public static SmsSendResult? FromHttpError(SmsHttpResponse response, Func<string, string> sanitize)
    {
        var code = (int)response.StatusCode;
        if (code is >= 200 and < 300) return null;

        var raw = sanitize(response.Body);
        return code is 429 or 502 or 503 or 504
            ? new SmsSendResult(SmsSendOutcome.Retryable, ErrorCode: $"HTTP {code}",
                ErrorMessage: code == 429 ? "Rate limited by the gateway." : "Gateway temporarily unavailable.",
                RawResponse: raw, RetryAfter: response.RetryAfter)
            : new SmsSendResult(SmsSendOutcome.Rejected, ErrorCode: $"HTTP {code}",
                ErrorMessage: $"Gateway returned HTTP {code}.", RawResponse: raw);
    }

    public static SmsSendResult FromTransport(SmsTransportException ex)
        => new(ex.Outcome, ErrorCode: ex.Outcome == SmsSendOutcome.Undetermined ? "TIMEOUT" : "UNREACHABLE", ErrorMessage: ex.Message);

    /// <summary>Truncates a response body for storage and blanks out any secret value it echoes back.</summary>
    public static string Sanitize(string? body, SmsProviderContext context)
    {
        if (string.IsNullOrEmpty(body)) return string.Empty;
        foreach (var secret in context.Secrets.Values.Where(s => s.Length >= 4))
            body = body.Replace(secret, "***", StringComparison.Ordinal);
        return body.Length > MaxStoredResponse ? body[..MaxStoredResponse] : body;
    }

    public static JsonElement? TryParseJson(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Follows a dot path ("data.request_id", "items.0.id") through a JSON value. Object keys match
    /// case-insensitively; a numeric segment indexes into an array.
    /// </summary>
    public static JsonElement? Select(JsonElement? root, string? path)
    {
        if (root is null || string.IsNullOrWhiteSpace(path)) return root;
        var current = root.Value;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind == JsonValueKind.Object)
            {
                var found = current.EnumerateObject().FirstOrDefault(p => string.Equals(p.Name, segment, StringComparison.OrdinalIgnoreCase));
                if (found.Value.ValueKind == JsonValueKind.Undefined) return null;
                current = found.Value;
            }
            else if (current.ValueKind == JsonValueKind.Array && int.TryParse(segment, out var index) && index < current.GetArrayLength())
            {
                current = current[index];
            }
            else
            {
                return null;
            }
        }
        return current;
    }

    /// <summary>A JSON value as text: strings unquoted, numbers and booleans as written, null for null/missing.</summary>
    public static string? AsText(JsonElement? value) => value?.ValueKind switch
    {
        JsonValueKind.String => value.Value.GetString(),
        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => value.Value.GetRawText(),
        _ => null,
    };

    /// <summary>A JSON number or numeric string ("5000.0000") as a decimal.</summary>
    public static decimal? AsDecimal(JsonElement? value)
    {
        var text = AsText(value);
        return decimal.TryParse(text, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : null;
    }
}

/// <summary>
/// Scrubs every vendor-supplied string in a provider result before it is stored, logged or
/// returned. Vendors do echo credentials back — BulkSMSBD's "user id not found in this {api_key}
/// key" is one — so this runs on every result, whichever provider produced it.
/// </summary>
public static class SmsResultRedaction
{
    public static string? Redact(string? text, SmsProviderContext context)
    {
        if (string.IsNullOrEmpty(text)) return text;
        foreach (var secret in context.Secrets.Values.Where(s => s.Length >= 4))
            text = text.Replace(secret, "***", StringComparison.Ordinal);
        return text;
    }

    public static SmsSendResult Redact(SmsSendResult r, SmsProviderContext c) => r with
    {
        ProviderMessageId = Redact(r.ProviderMessageId, c),
        ProviderStatus = Redact(r.ProviderStatus, c),
        ErrorCode = Redact(r.ErrorCode, c),
        ErrorMessage = Redact(r.ErrorMessage, c),
        RawResponse = Redact(r.RawResponse, c),
    };

    public static SmsBalanceResult Redact(SmsBalanceResult r, SmsProviderContext c) => r with
    {
        Unit = Redact(r.Unit, c),
        Error = Redact(r.Error, c),
    };

    public static SmsDeliveryResult Redact(SmsDeliveryResult r, SmsProviderContext c) => r with
    {
        ProviderStatus = Redact(r.ProviderStatus, c),
        Error = Redact(r.Error, c),
    };
}
