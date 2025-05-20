namespace OrgHub.Core.Interfaces.Identity;

public interface IUserPermissionRepository
{
    Task<List<string>> GetPermissionsAsync(int userId);
    Task AssignPermissionsAsync(int userId, List<string> permissions);
    Task RemovePermissionsAsync(int userId, List<string> permissions);

}
