using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Repositories;

public class AccessLogRepository : IAccessLogRepository
{
    private readonly AppDbContext _db;

    public AccessLogRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(UserAccessLog log)
    {
        _db.Set<UserAccessLog>().Add(log);
        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<UserAccessLog>> GetPagedAsync(int page, int pageSize, Guid? userId, string? module, string? action, DateTime? from, DateTime? to, bool? isSuccess, string? httpMethod)
    {
        var query = _db.Set<UserAccessLog>().AsQueryable();

        if (userId.HasValue) query = query.Where(l => l.UserId == userId);
        if (!string.IsNullOrWhiteSpace(module)) query = query.Where(l => l.Module == module);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(l => l.Action.Contains(action));
        if (!string.IsNullOrWhiteSpace(httpMethod)) query = query.Where(l => l.HttpMethod == httpMethod);
        if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(l => l.CreatedAt <= to.Value);
        if (isSuccess.HasValue) query = query.Where(l => l.IsSuccess == isSuccess.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<UserAccessLog> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
