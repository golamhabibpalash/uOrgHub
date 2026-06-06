using uOrgHub.Settings.Models.Entities;
using uOrgHub.Shared.Models;

namespace uOrgHub.Settings.Repositories;

public interface ISystemSettingRepository
{
    Task<SystemSetting?> GetByIdAsync(Guid id);
    Task<SystemSetting?> GetByKeyAsync(string key);
    Task<List<SystemSetting>> GetByCategoryAsync(string category);
    Task<List<SystemSetting>> GetAllActiveAsync();
    Task<PagedResult<SystemSetting>> GetPagedAsync(PaginationRequest request);
    Task<SystemSetting> CreateAsync(SystemSetting entity);
    Task<SystemSetting> UpdateAsync(SystemSetting entity);
    Task DeleteAsync(SystemSetting entity);
    Task<bool> KeyExistsAsync(string key, Guid? excludeId = null);
}
