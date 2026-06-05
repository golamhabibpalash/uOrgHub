using Microsoft.Extensions.Logging;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Auth.Repositories;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Services;

public class AccessLogService : IAccessLogService
{
    private readonly IAccessLogRepository _repo;
    private readonly IAccessLogQueue _queue;
    private readonly ILogger<AccessLogService> _logger;

    public AccessLogService(IAccessLogRepository repo, IAccessLogQueue queue, ILogger<AccessLogService> logger)
    {
        _repo = repo;
        _queue = queue;
        _logger = logger;
    }

    public Task LogAsync(UserAccessLog log)
    {
        if (!_queue.TryEnqueue(log))
            _logger.LogWarning("Access log queue rejected entry for {Endpoint}", log.Endpoint);
        return Task.CompletedTask;
    }

    public async Task<PagedResult<UserAccessLogDto>> GetLogsAsync(AccessLogFilterRequest request)
    {
        var paged = await _repo.GetPagedAsync(request);
        return Map(paged);
    }

    public async Task<List<UserAccessLogDto>> GetAllLogsExportAsync(AccessLogFilterRequest? request = null)
    {
        var logs = await _repo.GetAllAsync(request);
        return logs.Select(ToDto).ToList();
    }

    public async Task<PagedResult<UserAccessLogDto>> GetUserLogsAsync(Guid userId, AccessLogFilterRequest request)
    {
        var req = request with { UserId = userId };
        return await GetLogsAsync(req);
    }

    public Task<AccessLogSummaryDto> GetSummaryAsync() =>
        _repo.GetSummaryAsync(DateTime.UtcNow.Date);

    private static PagedResult<UserAccessLogDto> Map(PagedResult<UserAccessLog> paged) =>
        new()
        {
            Items = paged.Items.Select(ToDto).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize,
        };

    private static UserAccessLogDto ToDto(UserAccessLog l) => new(
        l.Id, l.UserId, l.Username, l.Action, l.Module, l.EntityType, l.EntityId,
        l.HttpMethod, l.Endpoint, l.ResponseStatusCode, l.IpAddress, l.UserAgent,
        l.DurationMs, l.IsSuccess, l.ErrorMessage, l.OldValues, l.NewValues, l.CreatedAt);
}
