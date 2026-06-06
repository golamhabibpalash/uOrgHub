using uOrgHub.Settings.DTOs;
using uOrgHub.Shared.Models;

namespace uOrgHub.Settings.Services;

public interface ISystemSettingService
{
    Task<PagedResult<SystemSettingResponseDto>> GetPagedAsync(PaginationRequest request);
    Task<SystemSettingResponseDto> GetByIdAsync(Guid id);
    Task<SystemSettingResponseDto> GetByKeyAsync(string key);
    Task<List<SystemSettingResponseDto>> GetByCategoryAsync(string category);
    Task<List<SystemSettingResponseDto>> GetAllActiveAsync();
    Task<string?> GetValueAsync(string key);
    Task<T?> GetTypedValueAsync<T>(string key);
    Task<SystemSettingResponseDto> CreateAsync(CreateSystemSettingDto dto, string createdBy);
    Task<SystemSettingResponseDto> UpdateAsync(Guid id, UpdateSystemSettingDto dto, string updatedBy);
    Task DeleteAsync(Guid id, string deletedBy);
    Task InvalidateCacheAsync();
}
