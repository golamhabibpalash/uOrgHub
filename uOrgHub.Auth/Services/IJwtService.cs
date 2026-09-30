using System.Security.Claims;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Services;

public interface IJwtService
{
    /// <summary>
    /// Identity, roles and active company only — never the user's permission list. Permissions are
    /// resolved server-side per request (IPermissionService); carrying them in the token pushed the
    /// Authorization header of multi-role users past reverse proxies' 8 KB header limit.
    /// </summary>
    string GenerateAccessToken(ApplicationUser user, List<string> roles, Guid? companyId = null);
    RefreshToken GenerateRefreshToken(Guid userId, string ipAddress, Guid? companyId = null);
    ClaimsPrincipal? ValidateToken(string token);
}
