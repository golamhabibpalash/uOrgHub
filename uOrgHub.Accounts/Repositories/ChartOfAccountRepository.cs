using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Repositories;

public class ChartOfAccountRepository : IChartOfAccountRepository
{
    private readonly AppDbContext _context;

    public ChartOfAccountRepository(AppDbContext context) => _context = context;

    private IQueryable<ChartOfAccount> BaseQuery()
        => _context.Set<ChartOfAccount>()
            .Include(x => x.AccountGroup)
            .Include(x => x.ParentAccount)
            .Where(x => !x.IsDeleted);

    public async Task<ChartOfAccount?> GetByIdAsync(Guid id)
        => await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PagedResult<ChartOfAccount>> GetAllAsync(PaginationRequest request)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x =>
                x.AccountName.Contains(request.Search) ||
                x.AccountCode.Contains(request.Search));

        query = request.SortDescending
            ? query.OrderByDescending(x => x.AccountCode)
            : query.OrderBy(x => x.AccountCode);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ChartOfAccount> { Items = items, TotalCount = totalCount, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<ChartOfAccount> CreateAsync(ChartOfAccount entity)
    {
        _context.Set<ChartOfAccount>().Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(x => x.AccountGroup).LoadAsync();
        if (entity.ParentAccountId.HasValue)
            await _context.Entry(entity).Reference(x => x.ParentAccount).LoadAsync();
        return entity;
    }

    public async Task<ChartOfAccount> UpdateAsync(ChartOfAccount entity)
    {
        _context.Set<ChartOfAccount>().Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Set<ChartOfAccount>().FindAsync(id);
        if (entity is null) return;
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await BaseQuery().AnyAsync(x => x.Id == id);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.AccountCode == code && (excludeId == null || x.Id != excludeId));

    public async Task<List<JournalEntryLine>> GetLedgerAsync(Guid accountId)
    {
        return await _context.Set<JournalEntryLine>()
            .Include(x => x.JournalEntry)
            .Include(x => x.Account)
            .Where(x => x.AccountId == accountId && !x.IsDeleted)
            .OrderBy(x => x.JournalEntry.EntryDate)
            .ThenBy(x => x.LineOrder)
            .ToListAsync();
    }
}