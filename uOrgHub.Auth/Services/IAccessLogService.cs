using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Services;

public interface IAccessLogService
{
    Task LogAsync(UserAccessLog log);
    Task<PagedResult<UserAccessLogDto>> GetLogsAsync(AccessLogFilterRequest request);
    Task<PagedResult<UserAccessLogDto>> GetUserLogsAsync(Guid userId, AccessLogFilterRequest request);
    Task<AccessLogSummaryDto> GetSummaryAsync();
}
