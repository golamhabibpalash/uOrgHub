using System.Security.Claims;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Services;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user, List<string> roles, List<string> claims);
    RefreshToken GenerateRefreshToken(Guid userId, string ipAddress);
    ClaimsPrincipal? ValidateToken(string token);
}
