using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Repositories;

public class FiscalYearRepository : IFiscalYearRepository
{
    private readonly AppDbContext _context;

    public FiscalYearRepository(AppDbContext context) => _context = context;

    private IQueryable<FiscalYear> BaseQuery()
        => _context.Set<FiscalYear>().Where(x => !x.IsDeleted);

    public async Task<FiscalYear?> GetByIdAsync(Guid id)
        => await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PagedResult<FiscalYear>> GetAllAsync(PaginationRequest request)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.WhereSearch(request.Search, x => x.Name);

        query = request.SortDescending
            ? query.OrderByDescending(x => x.StartDate)
            : query.OrderBy(x => x.StartDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<FiscalYear> { Items = items, TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<FiscalYear> CreateAsync(FiscalYear entity)
    {
        _context.Set<FiscalYear>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<FiscalYear> UpdateAsync(FiscalYear entity)
    {
        _context.Set<FiscalYear>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<FiscalYear>().FindAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await BaseQuery().AnyAsync(x => x.Id == id);

    public async Task<FiscalYear?> GetCurrentAsync()
        => await BaseQuery().FirstOrDefaultAsync(x => x.IsCurrent);

    public async Task<bool> SetCurrentAsync(Guid id)
    {
        var allCurrent = await BaseQuery().Where(x => x.IsCurrent).ToListAsync();
        foreach (var fy in allCurrent)
        {
            fy.IsCurrent = false;
        }

        var fiscalYear = await _context.Set<FiscalYear>().FindAsync(id);
        if (fiscalYear == null) return false;
        
        fiscalYear.IsCurrent = true;
        await _context.SaveChangesAsync();
        return true;
    }
}