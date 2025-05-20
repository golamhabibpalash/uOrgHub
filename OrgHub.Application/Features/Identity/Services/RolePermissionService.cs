using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Interfaces;
using OrgHub.Core.Interfaces.Identity;

namespace OrgHub.Application.Features.Identity.Services;

public class RolePermissionService : IRolePermissionService
{
    private readonly IRolePermissionRepository _rolePermissionRepository;
    public RolePermissionService(IRolePermissionRepository rolePermissionRepository)
    {
        _rolePermissionRepository = rolePermissionRepository;
    }
    public async Task<RolePermissionsDto> AssignPermissionsAsync(RolePermissionsDto dto)
    {
        await _rolePermissionRepository.AssignPermissionsAsync(dto.RoleId, (List<string>)dto.Permissions);
        var updated = await _rolePermissionRepository.GetPermissionsAsync(dto.RoleId);
        return new RolePermissionsDto { RoleId = dto.RoleId, Permissions = updated };

    }

    public async Task<RolePermissionsDto> GetPermissionsAsync(int userId)
    {
        var permissions = await _rolePermissionRepository.GetPermissionsAsync(userId);
        return new RolePermissionsDto { RoleId = userId, Permissions = permissions };
    }

    public async Task<RolePermissionsDto> RemovePermissionsAsync(RolePermissionsDto dto)
    {
        await _rolePermissionRepository.RemovePermissionsAsync(dto.RoleId, (List<string>)dto.Permissions);
        var updated = await _rolePermissionRepository.GetPermissionsAsync(dto.RoleId);
        return new RolePermissionsDto { RoleId = dto.RoleId, Permissions = updated };
    }
}
