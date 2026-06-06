using Microsoft.EntityFrameworkCore;
using uOrgHub.Settings.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.Settings.Repositories;

public class SystemSettingRepository : ISystemSettingRepository
{
    private readonly AppDbContext _db;

    public SystemSettingRepository(AppDbContext db) => _db = db;

    public async Task<SystemSetting?> GetByIdAsync(Guid id)
        => await _db.Set<SystemSetting>().FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

    public async Task<SystemSetting?> GetByKeyAsync(string key)
        => await _db.Set<SystemSetting>().FirstOrDefaultAsync(s => s.Key == key && !s.IsDeleted);

    public async Task<List<SystemSetting>> GetByCategoryAsync(string category)
        => await _db.Set<SystemSetting>().Where(s => s.Category == category && !s.IsDeleted).ToListAsync();

    public async Task<List<SystemSetting>> GetAllActiveAsync()
        => await _db.Set<SystemSetting>().Where(s => s.IsActive && !s.IsDeleted).ToListAsync();

    public async Task<PagedResult<SystemSetting>> GetPagedAsync(PaginationRequest request)
    {
        var query = _db.Set<SystemSetting>().Where(s => !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(s => s.Key.ToLower().Contains(search) || s.Category.ToLower().Contains(search) || s.Value.ToLower().Contains(search));
        }

        var total = await query.CountAsync();
        var items = await query.OrderBy(s => s.Category).ThenBy(s => s.Key)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync();

        return new PagedResult<SystemSetting> { Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<SystemSetting> CreateAsync(SystemSetting entity)
    {
        _db.Set<SystemSetting>().Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<SystemSetting> UpdateAsync(SystemSetting entity)
    {
        _db.Set<SystemSetting>().Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(SystemSetting entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<bool> KeyExistsAsync(string key, Guid? excludeId = null)
        => excludeId.HasValue
            ? await _db.Set<SystemSetting>().AnyAsync(s => s.Key == key && s.Id != excludeId.Value && !s.IsDeleted)
            : await _db.Set<SystemSetting>().AnyAsync(s => s.Key == key && !s.IsDeleted);
}
