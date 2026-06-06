using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using uOrgHub.Auth.Services;

namespace uOrgHub.API.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAnyClaimAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string[] ClaimNames { get; }

    public RequireAnyClaimAttribute(params string[] claimNames)
    {
        ClaimNames = claimNames;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userIdClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

        foreach (var claimName in ClaimNames)
        {
            if (await permissionService.HasClaimAsync(userId, claimName))
                return;
        }

        context.HttpContext.Items["AccessDeniedClaim"] = string.Join(" OR ", ClaimNames);
        context.Result = new ObjectResult(new
        {
            Success = false,
            Message = $"Access denied: one of [{string.Join(", ", ClaimNames)}] claims required"
        })
        {
            StatusCode = 403
        };
    }
}
