namespace OrgHub.Core.Interfaces.Identity;

public interface IRolePermissionRepository
{
    Task<List<string>> GetPermissionsAsync(int roleId);
    Task AssignPermissionsAsync(int roleId, List<string> permissions);
    Task RemovePermissionsAsync(int roleId, List<string> permissions);
}
