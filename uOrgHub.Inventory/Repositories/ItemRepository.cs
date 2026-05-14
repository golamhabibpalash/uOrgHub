using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public class ItemRepository : BaseRepository<Item>, IItemRepository
{
    public ItemRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<Item> ApplySearch(IQueryable<Item> query, string search)
        => query.Where(x => x.BaseName.Contains(search) || (x.ItemCode != null && x.ItemCode.Contains(search)));

    protected override IQueryable<Item> ApplyOrdering(IQueryable<Item> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.BaseName) : query.OrderBy(x => x.BaseName);

    public async Task<string> GenerateItemCodeAsync()
    {
        var count = await _context.Set<Item>().CountAsync() + 1;
        return $"ITM-{count:D6}";
    }
}
