using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Repositories;

public class AccessLogRepository : IAccessLogRepository
{
    private const int MaxPageSize = 200;

    private readonly AppDbContext _db;

    public AccessLogRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(UserAccessLog log)
    {
        _db.Set<UserAccessLog>().Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IReadOnlyCollection<UserAccessLog> logs, CancellationToken ct = default)
    {
        if (logs.Count == 0) return;
        await _db.Set<UserAccessLog>().AddRangeAsync(logs, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<UserAccessLog>> GetPagedAsync(AccessLogFilterRequest request)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);

        var query = _db.Set<UserAccessLog>().AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(l => l.UserId == request.UserId);

        if (!string.IsNullOrWhiteSpace(request.Username))
            query = query.Where(l => l.Username != null && l.Username.Contains(request.Username));

        if (!string.IsNullOrWhiteSpace(request.Module))
            query = query.Where(l => l.Module == request.Module);

        if (!string.IsNullOrWhiteSpace(request.Action))
            query = query.Where(l => l.Action.Contains(request.Action));

        if (!string.IsNullOrWhiteSpace(request.HttpMethod))
            query = query.Where(l => l.HttpMethod == request.HttpMethod);

        if (request.IsSuccess.HasValue)
            query = query.Where(l => l.IsSuccess == request.IsSuccess.Value);

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            query = query.Where(l => l.EntityType != null && l.EntityType.Contains(request.EntityType));

        if (!string.IsNullOrWhiteSpace(request.IpAddress))
            query = query.Where(l => l.IpAddress != null && l.IpAddress.Contains(request.IpAddress));

        if (request.StatusCodeFrom.HasValue)
            query = query.Where(l => l.ResponseStatusCode >= request.StatusCodeFrom.Value);

        if (request.StatusCodeTo.HasValue)
            query = query.Where(l => l.ResponseStatusCode <= request.StatusCodeTo.Value);

        if (request.DurationMin.HasValue)
            query = query.Where(l => l.DurationMs >= request.DurationMin.Value);

        if (request.DurationMax.HasValue)
            query = query.Where(l => l.DurationMs <= request.DurationMax.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(l => l.CreatedAt >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(l => l.CreatedAt <= request.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(l =>
                (l.Username != null && l.Username.Contains(request.Search)) ||
                (l.Action.Contains(request.Search)) ||
                (l.Module != null && l.Module.Contains(request.Search)) ||
                (l.Endpoint != null && l.Endpoint.Contains(request.Search)) ||
                (l.IpAddress != null && l.IpAddress.Contains(request.Search)));

        query = (request.SortBy?.ToLowerInvariant()) switch
        {
            "username" => request.SortDescending
                ? query.OrderByDescending(l => l.Username) : query.OrderBy(l => l.Username),
            "action" => request.SortDescending
                ? query.OrderByDescending(l => l.Action) : query.OrderBy(l => l.Action),
            "module" => request.SortDescending
                ? query.OrderByDescending(l => l.Module) : query.OrderBy(l => l.Module),
            "httpmethod" => request.SortDescending
                ? query.OrderByDescending(l => l.HttpMethod) : query.OrderBy(l => l.HttpMethod),
            "statuscode" => request.SortDescending
                ? query.OrderByDescending(l => l.ResponseStatusCode) : query.OrderBy(l => l.ResponseStatusCode),
            "duration" => request.SortDescending
                ? query.OrderByDescending(l => l.DurationMs) : query.OrderBy(l => l.DurationMs),
            "ip" => request.SortDescending
                ? query.OrderByDescending(l => l.IpAddress) : query.OrderBy(l => l.IpAddress),
            _ => request.SortDescending
                ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt),
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<UserAccessLog>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AccessLogSummaryDto> GetSummaryAsync(DateTime fromUtc, CancellationToken ct = default)
    {
        var q = _db.Set<UserAccessLog>().AsNoTracking().Where(l => l.CreatedAt >= fromUtc);

        var total = await q.LongCountAsync(ct);
        if (total == 0)
            return new AccessLogSummaryDto(0, 0, 0, 0, 0, 0);

        var failed = await q.LongCountAsync(l => !l.IsSuccess, ct);
        var avgDurationMs = await q.AverageAsync(l => (double)l.DurationMs, ct);
        var uniqueUsers = await q
            .Where(l => l.UserId != null)
            .Select(l => l.UserId)
            .Distinct()
            .CountAsync(ct);

        var failureRate = (double)failed / total * 100;
        return new AccessLogSummaryDto(total, failed, failureRate, avgDurationMs, uniqueUsers, 0);
    }

    public Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default) =>
        _db.Set<UserAccessLog>()
            .Where(l => l.CreatedAt < cutoffUtc)
            .ExecuteDeleteAsync(ct);
}
