using uOrgHub.Settings.Models.Entities;
using uOrgHub.Shared.Models;

namespace uOrgHub.Settings.Repositories;

public interface IValidationRuleRepository
{
    Task<ValidationRule?> GetByIdAsync(Guid id);
    Task<List<ValidationRule>> GetByEntityTypeAsync(string entityType);
    Task<List<ValidationRule>> GetAllEnabledAsync();
    Task<PagedResult<ValidationRule>> GetPagedAsync(PaginationRequest request);
    Task<ValidationRule> CreateAsync(ValidationRule entity);
    Task<ValidationRule> UpdateAsync(ValidationRule entity);
    Task DeleteAsync(ValidationRule entity);
}
