using Microsoft.AspNetCore.Identity;
using OrgHub.Core.Interfaces.Identity;
using System.Security.Claims;

namespace OrgHub.Infrastructure.Repositories.Identity;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly RoleManager<IdentityUser> _roleManager;
    public RolePermissionRepository(RoleManager<IdentityUser> roleManager)
    {
        _roleManager = roleManager;
    }
    public async Task AssignPermissionsAsync(int roleId, List<string> permissions)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        foreach (var permission in permissions)
        {
            if (!(await _roleManager.GetClaimsAsync(role)).Any(c => c.Type == "Permission" && c.Value == permission))
            {
                await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }
        }
    }

    public async Task<List<string>> GetPermissionsAsync(int roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        var claims = await _roleManager.GetClaimsAsync(role);
        return claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();
    }

    public async Task RemovePermissionsAsync(int roleId, List<string> permissions)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        var claims = await _roleManager.GetClaimsAsync(role);
        foreach (var permission in permissions)
        {
            var claim = claims.FirstOrDefault(c => c.Type == "Permission" && c.Value == permission);
            if (claim != null)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }
        }
    }
}
