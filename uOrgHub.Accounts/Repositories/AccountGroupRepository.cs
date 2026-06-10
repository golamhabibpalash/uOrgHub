using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
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
            query = query.WhereSearch(request.Search, x => x.Name, x => x.Code);

        query = query.ApplySorting(request.SortBy ?? "Name", request.SortDescending);

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

    private static readonly Dictionary<AccountGroupType, int> _typePrefix = new()
    {
        { AccountGroupType.Asset, 1 },
        { AccountGroupType.Liability, 2 },
        { AccountGroupType.Equity, 3 },
        { AccountGroupType.Income, 4 },
        { AccountGroupType.Expense, 5 },
    };

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId));

    public async Task<bool> CustomCodeExistsAsync(string customCode, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.CustomCode == customCode && (excludeId == null || x.Id != excludeId));

    public async Task<string> GetNextCodeAsync(AccountGroupType type, Guid? parentId)
    {
        var prefix = _typePrefix[type];
        var rangeStart = prefix * 1000;
        var rangeEnd = rangeStart + 999;

        if (!parentId.HasValue)
        {
            var taken = await BaseQuery()
                .Where(x => x.Type == type && x.Code.Length == 4 && x.Code.EndsWith("00"))
                .Select(x => x.Code)
                .ToListAsync();

            var used = taken.Select(int.Parse).ToHashSet();
            for (var code = rangeStart; code <= rangeEnd; code += 100)
            {
                if (!used.Contains(code))
                    return code.ToString();
            }
        }
        else
        {
            var parent = await BaseQuery().FirstOrDefaultAsync(x => x.Id == parentId.Value);
            if (parent == null || !int.TryParse(parent.Code, out var parentCode))
                return (rangeStart).ToString();

            var step = parentCode % 100 == 0 ? 100 : parentCode % 10 == 0 ? 10 : 1;
            var basePrefix = parentCode / step * step;
            var start = basePrefix + step;

            var taken = await BaseQuery()
                .Where(x => x.ParentAccountGroupId == parentId.Value)
                .Select(x => x.Code)
                .ToListAsync();

            var used = taken.Select(int.Parse).ToHashSet();
            for (var code = start; code <= rangeEnd; code += step)
            {
                if (!used.Contains(code))
                    return code.ToString();
            }
        }

        return (rangeEnd).ToString();
    }

    public async Task<List<AccountGroup>> GetAllFlatAsync()
        => await BaseQuery().OrderBy(x => x.Code).ToListAsync();

    public async Task<List<Guid>> GetDescendantIdsAsync(Guid id)
    {
        var all = await BaseQuery().Select(x => new IdPair(x.Id, x.ParentAccountGroupId)).ToListAsync();
        var lookup = all.ToLookup(x => x.ParentId);
        var result = new List<Guid>();
        CollectDescendants(id, lookup, result);
        return result;
    }

    private static void CollectDescendants(Guid parentId, ILookup<Guid?, IdPair> lookup, List<Guid> result)
    {
        foreach (var child in lookup[parentId])
        {
            result.Add(child.Id);
            CollectDescendants(child.Id, lookup, result);
        }
    }

    private record IdPair(Guid Id, Guid? ParentId);

    public async Task<bool> HasChildrenAsync(Guid id)
        => await BaseQuery().AnyAsync(x => x.ParentAccountGroupId == id);
}