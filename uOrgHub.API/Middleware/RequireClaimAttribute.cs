using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using uOrgHub.Auth.Services;

namespace uOrgHub.API.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireClaimAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string ClaimName { get; }

    public RequireClaimAttribute(string claimName)
    {
        ClaimName = claimName;
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
        var hasClaim = await permissionService.HasClaimAsync(userId, ClaimName);

        if (!hasClaim)
        {
            context.Result = new ObjectResult(new
            {
                Success = false,
                Message = $"Access denied: {ClaimName} claim required"
            })
            {
                StatusCode = 403
            };
        }
    }
}
