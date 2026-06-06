using uOrgHub.Settings.DTOs;
using uOrgHub.Settings.Models.Entities;
using uOrgHub.Settings.Repositories;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Settings.Services;

public class ValidationRuleService : IValidationRuleService
{
    private readonly IValidationRuleRepository _repo;

    public ValidationRuleService(IValidationRuleRepository repo) => _repo = repo;

    public async Task<PagedResult<ValidationRuleResponseDto>> GetPagedAsync(PaginationRequest request)
    {
        var result = await _repo.GetPagedAsync(request);
        return new PagedResult<ValidationRuleResponseDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
        };
    }

    public async Task<ValidationRuleResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("ValidationRule", id);
        return MapToDto(entity);
    }

    public async Task<List<ValidationRuleResponseDto>> GetByEntityTypeAsync(string entityType)
    {
        var entities = await _repo.GetByEntityTypeAsync(entityType);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<ValidationRuleResponseDto> CreateAsync(CreateValidationRuleDto dto, string createdBy)
    {
        var entity = new ValidationRule
        {
            EntityType = dto.EntityType,
            FieldName = dto.FieldName,
            RuleType = dto.RuleType,
            RuleValue = dto.RuleValue,
            ErrorMessage = dto.ErrorMessage,
            Severity = dto.Severity,
            IsEnabled = dto.IsEnabled,
            SortOrder = dto.SortOrder,
            CreatedBy = createdBy,
        };

        entity = await _repo.CreateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<ValidationRuleResponseDto> UpdateAsync(Guid id, UpdateValidationRuleDto dto, string updatedBy)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("ValidationRule", id);

        entity.EntityType = dto.EntityType;
        entity.FieldName = dto.FieldName;
        entity.RuleType = dto.RuleType;
        entity.RuleValue = dto.RuleValue;
        entity.ErrorMessage = dto.ErrorMessage;
        entity.Severity = dto.Severity;
        entity.IsEnabled = dto.IsEnabled;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;

        entity = await _repo.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id, string deletedBy)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("ValidationRule", id);
        entity.DeletedBy = deletedBy;
        await _repo.DeleteAsync(entity);
    }

    private static ValidationRuleResponseDto MapToDto(ValidationRule r) => new(
        r.Id, r.EntityType, r.FieldName, r.RuleType, r.RuleValue, r.ErrorMessage,
        r.Severity, r.IsEnabled, r.SortOrder, r.CreatedAt, r.CreatedBy, r.UpdatedAt, r.UpdatedBy
    );
}
