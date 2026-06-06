using uOrgHub.Settings.DTOs;
using uOrgHub.Shared.Models;

namespace uOrgHub.Settings.Services;

public interface IValidationRuleService
{
    Task<PagedResult<ValidationRuleResponseDto>> GetPagedAsync(PaginationRequest request);
    Task<ValidationRuleResponseDto> GetByIdAsync(Guid id);
    Task<List<ValidationRuleResponseDto>> GetByEntityTypeAsync(string entityType);
    Task<ValidationRuleResponseDto> CreateAsync(CreateValidationRuleDto dto, string createdBy);
    Task<ValidationRuleResponseDto> UpdateAsync(Guid id, UpdateValidationRuleDto dto, string updatedBy);
    Task DeleteAsync(Guid id, string deletedBy);
}
