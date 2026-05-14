using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Repositories;

public interface IRoleRepository
{
    Task<List<ApplicationRole>> GetAllWithClaimsAsync();
    Task<ApplicationRole?> GetByIdAsync(Guid id);
    Task<ApplicationRole?> GetByIdWithClaimsAsync(Guid id);
    Task<ApplicationRole?> GetByNameAsync(string name);
    Task<ApplicationRole> AddAsync(ApplicationRole role);
    Task UpdateAsync(ApplicationRole role);
    Task SoftDeleteAsync(ApplicationRole role, string deletedBy);
    Task SetRoleClaimsAsync(Guid roleId, List<Guid> claimIds, string assignedBy);
    Task AddRoleClaimAsync(Guid roleId, Guid claimId, string assignedBy);
    Task RemoveRoleClaimAsync(Guid roleId, Guid claimId);
    Task<int> GetUserCountAsync(Guid roleId);
}
