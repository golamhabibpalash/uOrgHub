using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Repositories;

public interface IAccessLogRepository
{
    Task AddAsync(UserAccessLog log);
    Task AddRangeAsync(IReadOnlyCollection<UserAccessLog> logs, CancellationToken ct = default);
    Task<PagedResult<UserAccessLog>> GetPagedAsync(AccessLogFilterRequest request);
    Task<List<UserAccessLog>> GetAllAsync(AccessLogFilterRequest? filter = null);
    Task<AccessLogSummaryDto> GetSummaryAsync(DateTime fromUtc, CancellationToken ct = default);
    Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default);
}
