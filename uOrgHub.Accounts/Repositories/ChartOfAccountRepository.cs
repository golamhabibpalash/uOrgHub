using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
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
            query = query.WhereSearch(request.Search, x => x.AccountName, x => x.AccountCode);

        query = query.ApplySorting(request.SortBy ?? "AccountCode", request.SortDescending);

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

    public async Task<bool> CustomCodeExistsAsync(string customCode, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.CustomCode == customCode && (excludeId == null || x.Id != excludeId));

    public async Task<string> GetNextAccountCodeAsync(Guid accountGroupId)
    {
        var group = await _context.Set<AccountGroup>()
            .Where(x => x.Id == accountGroupId && !x.IsDeleted)
            .Select(x => x.Code)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(group)) return "000001";

        var prefix = group;
        var taken = await BaseQuery()
            .Where(x => x.AccountCode.StartsWith(prefix) && x.AccountCode.Length == prefix.Length + 2)
            .Select(x => x.AccountCode)
            .ToListAsync();

        var used = taken
            .Select(x => int.TryParse(x[prefix.Length..], out var n) ? n : 0)
            .ToHashSet();

        for (var i = 1; i <= 99; i++)
        {
            if (!used.Contains(i))
                return $"{prefix}{i:D2}";
        }

        return $"{prefix}99";
    }

    public async Task<AccountGroupType?> GetAccountGroupTypeAsync(Guid accountGroupId)
    {
        return await _context.Set<AccountGroup>()
            .Where(x => x.Id == accountGroupId && !x.IsDeleted)
            .Select(x => (AccountGroupType?)x.Type)
            .FirstOrDefaultAsync();
    }

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