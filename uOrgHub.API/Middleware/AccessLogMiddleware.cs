using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Auth.Services;

namespace uOrgHub.API.Middleware;

public class AccessLogMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AccessLogMiddleware> _logger;

    private static readonly string[] SkipPaths = { "/health", "/swagger", "/favicon" };

    public AccessLogMiddleware(RequestDelegate next, ILogger<AccessLogMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAccessLogService accessLogService)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (SkipPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        string? requestBody = null;
        if (context.Request.Method is "POST" or "PUT" or "PATCH" && context.Request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            requestBody = SanitizeRequestBody(requestBody);
            context.Request.Body.Position = 0;
        }

        string? errorMessage = null;
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            stopwatch.Stop();
            await LogAsync(context, accessLogService, stopwatch.ElapsedMilliseconds, requestBody, errorMessage);
            throw;
        }

        stopwatch.Stop();

        await LogAsync(context, accessLogService, stopwatch.ElapsedMilliseconds, requestBody, errorMessage);
    }

    private async Task LogAsync(HttpContext context, IAccessLogService accessLogService, long durationMs, string? requestBody, string? errorMessage)
    {
        try
        {
            var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = userIdClaim != null && Guid.TryParse(userIdClaim, out var uid) ? uid : null;

            var username = context.User?.FindFirst("username")?.Value;

            var deniedClaim = context.Items["AccessDeniedClaim"] as string;
            var status = context.Response.StatusCode;
            var effectiveError = errorMessage
                ?? (deniedClaim != null ? $"Missing claim: {deniedClaim}" : null);

            var log = new UserAccessLog
            {
                UserId = userId,
                Username = username,
                Action = GetActionName(context),
                Module = GetModuleFromPath(context.Request.Path),
                EntityType = null,
                EntityId = null,
                HttpMethod = context.Request.Method,
                Endpoint = context.Request.Path,
                RequestBody = requestBody,
                ResponseStatusCode = status,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                DurationMs = durationMs,
                IsSuccess = effectiveError == null && status < 400,
                ErrorMessage = effectiveError,
            };

            await accessLogService.LogAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log access entry");
        }
    }

    private static string GetActionName(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "";

        return method switch
        {
            "GET" => "View",
            "POST" => "Create",
            "PUT" => "Update",
            "PATCH" => "Modify",
            "DELETE" => "Delete",
            _ => method,
        } + " " + path;
    }

    private static string? GetModuleFromPath(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments == null || segments.Length < 2) return null;

        // api/v1/{module}/...
        if (segments.Length >= 3 && segments[0] == "api" && segments[1] == "v1")
            return segments[2];

        return segments[0];
    }

    private static string? SanitizeRequestBody(string? body)
    {
        if (string.IsNullOrEmpty(body)) return body;

        var sensitiveFields = new[] { "password", "currentPassword", "newPassword", "confirmPassword", "token", "secret" };
        foreach (var field in sensitiveFields)
        {
            var pattern = $"\"{field}\":\"[^\"]*\"";
            body = System.Text.RegularExpressions.Regex.Replace(body, pattern, $"\"{field}\":\"***\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return body?.Length > 2000 ? body[..2000] + "..." : body;
    }
}
