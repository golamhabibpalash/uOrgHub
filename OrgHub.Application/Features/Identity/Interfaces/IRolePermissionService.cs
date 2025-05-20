using OrgHub.Application.Features.Identity.DTOs;

namespace OrgHub.Application.Features.Identity.Interfaces;

public interface IRolePermissionService
{
    Task<RolePermissionsDto> AssignPermissionsAsync(RolePermissionsDto dto);
    Task<RolePermissionsDto> RemovePermissionsAsync(RolePermissionsDto dto);
    Task<RolePermissionsDto> GetPermissionsAsync(int userId);
}
