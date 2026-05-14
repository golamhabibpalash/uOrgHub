using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Repositories;

public interface IClaimRepository
{
    Task<List<ApplicationClaim>> GetAllAsync(string? module = null);
    Task<ApplicationClaim?> GetByIdAsync(Guid id);
    Task<ApplicationClaim?> GetByNameAsync(string name);
    Task<ApplicationClaim> AddAsync(ApplicationClaim claim);
    Task UpdateAsync(ApplicationClaim claim);
    Task SoftDeleteAsync(ApplicationClaim claim, string deletedBy);
}
