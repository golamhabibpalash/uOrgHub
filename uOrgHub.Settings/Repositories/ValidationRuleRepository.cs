using Microsoft.EntityFrameworkCore;
using uOrgHub.Settings.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.Settings.Repositories;

public class ValidationRuleRepository : IValidationRuleRepository
{
    private readonly AppDbContext _db;

    public ValidationRuleRepository(AppDbContext db) => _db = db;

    public async Task<ValidationRule?> GetByIdAsync(Guid id)
        => await _db.Set<ValidationRule>().FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

    public async Task<List<ValidationRule>> GetByEntityTypeAsync(string entityType)
        => await _db.Set<ValidationRule>()
            .Where(r => r.EntityType == entityType && r.IsEnabled && !r.IsDeleted)
            .OrderBy(r => r.SortOrder)
            .ToListAsync();

    public async Task<List<ValidationRule>> GetAllEnabledAsync()
        => await _db.Set<ValidationRule>().Where(r => r.IsEnabled && !r.IsDeleted).OrderBy(r => r.EntityType).ThenBy(r => r.FieldName).ThenBy(r => r.SortOrder).ToListAsync();

    public async Task<PagedResult<ValidationRule>> GetPagedAsync(PaginationRequest request)
    {
        var query = _db.Set<ValidationRule>().Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(r => r.EntityType.ToLower().Contains(search) || r.FieldName.ToLower().Contains(search) || r.RuleType.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(r => r.EntityType).ThenBy(r => r.FieldName).ThenBy(r => r.SortOrder)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync();

        return new PagedResult<ValidationRule> { Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<ValidationRule> CreateAsync(ValidationRule entity)
    {
        _db.Set<ValidationRule>().Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<ValidationRule> UpdateAsync(ValidationRule entity)
    {
        _db.Set<ValidationRule>().Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(ValidationRule entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
