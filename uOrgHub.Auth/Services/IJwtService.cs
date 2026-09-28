using System.Security.Claims;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Services;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, List<string> roles, List<string> claims, Guid? companyId = null);
    RefreshToken GenerateRefreshToken(Guid userId, string ipAddress, Guid? companyId = null);
    ClaimsPrincipal? ValidateToken(string token);
}
