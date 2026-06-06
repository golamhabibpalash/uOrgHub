using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using uOrgHub.Settings.DTOs;
using uOrgHub.Settings.Models.Entities;
using uOrgHub.Settings.Repositories;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Settings.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly ISystemSettingRepository _repo;
    private readonly ILogger<SystemSettingService> _logger;
    private static readonly ConcurrentDictionary<string, string?> _cache = new();
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly object _cacheLock = new();

    public SystemSettingService(ISystemSettingRepository repo, ILogger<SystemSettingService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<PagedResult<SystemSettingResponseDto>> GetPagedAsync(PaginationRequest request)
    {
        var result = await _repo.GetPagedAsync(request);
        return new PagedResult<SystemSettingResponseDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
        };
    }

    public async Task<SystemSettingResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("SystemSetting", id);
        return MapToDto(entity);
    }

    public async Task<SystemSettingResponseDto> GetByKeyAsync(string key)
    {
        var entity = await _repo.GetByKeyAsync(key) ?? throw new AppException($"SystemSetting with key '{key}' was not found.", 404);
        return MapToDto(entity);
    }

    public async Task<List<SystemSettingResponseDto>> GetByCategoryAsync(string category)
    {
        var entities = await _repo.GetByCategoryAsync(category);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<SystemSettingResponseDto>> GetAllActiveAsync()
    {
        var entities = await _repo.GetAllActiveAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<string?> GetValueAsync(string key)
    {
        await EnsureCacheLoadedAsync();
        return _cache.GetValueOrDefault(key);
    }

    public async Task<T?> GetTypedValueAsync<T>(string key)
    {
        var value = await GetValueAsync(key);
        if (value is null) return default;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            _logger.LogWarning("Failed to convert setting '{Key}' value '{Value}' to {Type}", key, value, typeof(T).Name);
            return default;
        }
    }

    public async Task<SystemSettingResponseDto> CreateAsync(CreateSystemSettingDto dto, string createdBy)
    {
        if (await _repo.KeyExistsAsync(dto.Key))
            throw new AppException($"A setting with key '{dto.Key}' already exists.", 409);

        var entity = new SystemSetting
        {
            Category = dto.Category,
            Key = dto.Key,
            Value = dto.Value,
            DataType = dto.DataType,
            Description = dto.Description,
            IsActive = dto.IsActive,
            IsSystem = dto.IsSystem,
            CreatedBy = createdBy,
        };

        entity = await _repo.CreateAsync(entity);
        InvalidateCache();
        return MapToDto(entity);
    }

    public async Task<SystemSettingResponseDto> UpdateAsync(Guid id, UpdateSystemSettingDto dto, string updatedBy)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("SystemSetting", id);

        if (entity.IsSystem)
            throw new AppException("System settings cannot be modified.", 403);

        if (await _repo.KeyExistsAsync(dto.Key, id))
            throw new AppException($"A setting with key '{dto.Key}' already exists.", 409);

        entity.Category = dto.Category;
        entity.Key = dto.Key;
        entity.Value = dto.Value;
        entity.DataType = dto.DataType;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = updatedBy;

        entity = await _repo.UpdateAsync(entity);
        InvalidateCache();
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id, string deletedBy)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("SystemSetting", id);

        if (entity.IsSystem)
            throw new AppException("System settings cannot be deleted.", 403);

        entity.DeletedBy = deletedBy;
        await _repo.DeleteAsync(entity);
        InvalidateCache();
    }

    public Task InvalidateCacheAsync()
    {
        InvalidateCache();
        return Task.CompletedTask;
    }

    private void InvalidateCache()
    {
        lock (_cacheLock)
        {
            _cache.Clear();
            _cacheExpiry = DateTime.MinValue;
        }
    }

    private async Task EnsureCacheLoadedAsync()
    {
        if (DateTime.UtcNow < _cacheExpiry) return;

        var settings = await _repo.GetAllActiveAsync();
        lock (_cacheLock)
        {
            if (DateTime.UtcNow < _cacheExpiry) return;
            _cache.Clear();
            foreach (var s in settings)
                _cache[s.Key] = s.Value;
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
        }
    }

    private static SystemSettingResponseDto MapToDto(SystemSetting s) => new(
        s.Id, s.Category, s.Key, s.Value, s.DataType, s.Description,
        s.IsActive, s.IsSystem, s.CreatedAt, s.CreatedBy, s.UpdatedAt, s.UpdatedBy
    );
}
