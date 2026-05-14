using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Repositories;

public class AccountGroupRepository : IAccountGroupRepository
{
    private readonly AppDbContext _context;

    public AccountGroupRepository(AppDbContext context) => _context = context;

    private IQueryable<AccountGroup> BaseQuery()
        => _context.Set<AccountGroup>().Where(x => !x.IsDeleted);

    public async Task<AccountGroup?> GetByIdAsync(Guid id)
        => await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PagedResult<AccountGroup>> GetAllAsync(PaginationRequest request)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x =>
                x.Name.Contains(request.Search) ||
                x.Code.Contains(request.Search));

        query = request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<AccountGroup> { Items = items, TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<AccountGroup> CreateAsync(AccountGroup entity)
    {
        _context.Set<AccountGroup>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<AccountGroup> UpdateAsync(AccountGroup entity)
    {
        _context.Set<AccountGroup>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<AccountGroup>().FindAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await BaseQuery().AnyAsync(x => x.Id == id);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId));
}