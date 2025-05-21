using Microsoft.AspNetCore.Identity;
using OrgHub.Core.Interfaces.Identity;
using OrgHub.Domain.Entities.Identity;
using System.Security.Claims;

namespace OrgHub.Infrastructure.Repositories.Identity;

public class UserPermissionRepository : IUserPermissionRepository
{
    private readonly UserManager<User> _userManager;
    public UserPermissionRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    public async Task<List<string>> GetPermissionsAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
    }

    public async Task AssignPermissionsAsync(int userId, List<string> permissions)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        foreach (var permission in permissions)
        {
            if (!(await _userManager.GetClaimsAsync(user)).Any(c => c.Type == "Permission" && c.Value == permission))
            {
                await _userManager.AddClaimAsync(user, new Claim("Permission", permission));
            }
        }
    }

    public async Task RemovePermissionsAsync(int userId, List<string> permissions)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var claims = await _userManager.GetClaimsAsync(user);
        foreach (var permission in permissions)
        {
            var claim = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == permission);
            if (claim != null)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }
        }
    }
}
