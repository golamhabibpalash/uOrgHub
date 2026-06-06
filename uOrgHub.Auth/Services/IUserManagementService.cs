using uOrgHub.Auth.DTOs;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Services;

public interface IUserManagementService
{
    Task<UserDto> CreateUserAsync(CreateUserDto dto, string createdBy);
    Task SetUserActiveAsync(Guid userId, bool isActive, string updatedBy);
    Task SoftDeleteUserAsync(Guid userId, string deletedBy);
    Task UnlockUserAsync(Guid userId);
    Task ForceLogoutUserAsync(Guid userId, string reason);
    Task<PagedResult<UserDto>> GetUsersAsync(PaginationRequest request);
    Task<List<UserDto>> GetAllUsersExportAsync();
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task UpdateUserAsync(Guid id, UpdateUserDto dto, string updatedBy);
    Task<UserDto> ChangeUsernameAsync(Guid id, string newUsername, string updatedBy);
    Task AssignRoleAsync(Guid userId, Guid roleId, string assignedBy);
    Task RemoveRoleAsync(Guid userId, Guid roleId);
    Task ReplaceRolesAsync(Guid userId, List<Guid> roleIds, string assignedBy);
    Task AssignUserClaimAsync(Guid userId, Guid claimId, bool isGranted, string assignedBy);
    Task RemoveUserClaimAsync(Guid userId, Guid claimId);
    Task<List<UserSessionDto>> GetUserSessionsAsync(Guid userId);
    Task<List<UserAccessLogDto>> GetUserAccessLogsAsync(Guid userId, AccessLogFilterRequest request);
}
