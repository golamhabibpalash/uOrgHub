using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Interfaces;
using OrgHub.Core.Interfaces.Identity;

namespace OrgHub.Application.Features.Identity.Services;

public class UserPermissionService : IUserPermissionService
{
    private readonly IUserPermissionRepository _repository;

    public UserPermissionService(IUserPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserPermissionsDto> AssignPermissionsAsync(UserPermissionsDto dto)
    {
        await _repository.AssignPermissionsAsync(dto.UserId, dto.Permissions);
        var updated = await _repository.GetPermissionsAsync(dto.UserId);
        return new UserPermissionsDto { UserId = dto.UserId, Permissions = updated };
    }

    public async Task<UserPermissionsDto> RemovePermissionsAsync(UserPermissionsDto dto)
    {
        await _repository.RemovePermissionsAsync(dto.UserId, dto.Permissions);
        var updated = await _repository.GetPermissionsAsync(dto.UserId);
        return new UserPermissionsDto { UserId = dto.UserId, Permissions = updated };
    }

    public async Task<UserPermissionsDto> GetPermissionsAsync(int userId)
    {
        var permissions = await _repository.GetPermissionsAsync(userId);
        return new UserPermissionsDto { UserId = userId, Permissions = permissions };
    }

}
