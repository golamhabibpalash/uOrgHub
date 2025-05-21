using Serilog;
using System.Diagnostics;
using System.Security.Claims;

namespace OrgHub.Api.MiddleWares;

/// <summary>
/// Middleware for logging HTTP activity, including request method, path, response status code, elapsed time, and user ID.
/// </summary>
public class ActivityLoggingMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public ActivityLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware to log HTTP activity.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();

        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Log.Information("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms. UserId: {UserId}",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, userId);
    }
}
