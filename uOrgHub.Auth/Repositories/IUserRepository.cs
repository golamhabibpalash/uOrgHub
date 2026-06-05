using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByUsernameAsync(string username);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByIdAsync(Guid id);
    Task<ApplicationUser?> GetByEmployeeIdAsync(Guid employeeId);
    Task<ApplicationUser?> GetByIdWithDetailsAsync(Guid id);
    Task<PagedResult<ApplicationUser>> GetPagedAsync(int page, int pageSize, string? search);
    Task<ApplicationUser> AddAsync(ApplicationUser user);
    Task UpdateAsync(ApplicationUser user);
    Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);
    Task<IEnumerable<(string Name, bool IsGranted)>> GetUserClaimsAsync(Guid userId);
    Task SetUserRolesAsync(Guid userId, List<Guid> roleIds, string assignedBy);
    Task AddUserClaimAsync(Guid userId, Guid claimId, bool isGranted, string assignedBy);
    Task RemoveUserClaimAsync(Guid userId, Guid claimId);
}
