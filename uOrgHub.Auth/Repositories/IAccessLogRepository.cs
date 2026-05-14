using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Repositories;

public interface IAccessLogRepository
{
    Task AddAsync(UserAccessLog log);
    Task<PagedResult<UserAccessLog>> GetPagedAsync(int page, int pageSize, Guid? userId, string? module, string? action, DateTime? from, DateTime? to, bool? isSuccess, string? httpMethod);
}
