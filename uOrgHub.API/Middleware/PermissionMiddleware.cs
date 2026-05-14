using System.Security.Claims;
using uOrgHub.Auth.Services;

namespace uOrgHub.API.Middleware;

public class PermissionMiddleware
{
    private readonly RequestDelegate _next;

    public PermissionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPermissionService permissionService)
    {
        var endpoint = context.GetEndpoint();
        var requireClaim = endpoint?.Metadata.GetMetadata<RequireClaimAttribute>();

        if (requireClaim != null)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var hasClaim = await permissionService.HasClaimAsync(userId, requireClaim.ClaimName);
            if (!hasClaim)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Success = false,
                        Message = $"Access denied: {requireClaim.ClaimName} claim required"
                    }));
                return;
            }
        }

        await _next(context);
    }
}
