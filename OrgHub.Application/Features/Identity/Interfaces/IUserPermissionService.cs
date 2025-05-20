using OrgHub.Application.Features.Identity.DTOs;

namespace OrgHub.Application.Features.Identity.Interfaces;

public interface IUserPermissionService
{
    Task<UserPermissionsDto> AssignPermissionsAsync(UserPermissionsDto dto);
    Task<UserPermissionsDto> RemovePermissionsAsync(UserPermissionsDto dto);
    Task<UserPermissionsDto> GetPermissionsAsync(int userId);
}
