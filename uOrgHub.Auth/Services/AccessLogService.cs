using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Auth.Repositories;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Services;

public class AccessLogService : IAccessLogService
{
    private readonly IAccessLogRepository _repo;

    public AccessLogService(IAccessLogRepository repo) => _repo = repo;

    public async Task LogAsync(UserAccessLog log) => await _repo.AddAsync(log);

    public async Task<PagedResult<UserAccessLogDto>> GetLogsAsync(AccessLogFilterRequest request)
    {
        var paged = await _repo.GetPagedAsync(
            request.Page, request.PageSize, request.UserId, request.Module,
            request.Action, request.DateFrom, request.DateTo, request.IsSuccess, request.HttpMethod);

        return Map(paged);
    }

    public async Task<PagedResult<UserAccessLogDto>> GetUserLogsAsync(Guid userId, AccessLogFilterRequest request)
    {
        var req = request with { UserId = userId };
        return await GetLogsAsync(req);
    }

    public async Task<AccessLogSummaryDto> GetSummaryAsync()
    {
        // Summary uses today's data from a basic query
        var today = DateTime.UtcNow.Date;
        var paged = await _repo.GetPagedAsync(1, int.MaxValue, null, null, null, today, null, null, null);

        var total = paged.TotalCount;
        var failed = paged.Items.Count(l => !l.IsSuccess);
        var failureRate = total > 0 ? (double)failed / total * 100 : 0;
        var avgDuration = paged.Items.Count > 0 ? paged.Items.Average(l => (double)l.DurationMs) : 0;
        var uniqueUsers = paged.Items.Where(l => l.UserId.HasValue).Select(l => l.UserId!.Value).Distinct().Count();

        return new AccessLogSummaryDto(total, failed, failureRate, avgDuration, uniqueUsers, 0);
    }

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
