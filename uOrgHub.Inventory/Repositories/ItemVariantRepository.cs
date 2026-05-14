using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public class ItemVariantRepository : BaseRepository<ItemVariant>, IItemVariantRepository
{
    public ItemVariantRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<ItemVariant> ApplySearch(IQueryable<ItemVariant> query, string search)
        => query.Where(x => x.SKU.Contains(search) || x.VariantName.Contains(search));

    protected override IQueryable<ItemVariant> ApplyOrdering(IQueryable<ItemVariant> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.SKU) : query.OrderBy(x => x.SKU);

    public async Task<List<ItemVariant>> GetByItemIdAsync(Guid itemId)
        => await BaseQuery()
            .Include(x => x.Attributes).ThenInclude(x => x.AttributeDefinition)
            .Where(x => x.ItemId == itemId)
            .ToListAsync();

    public async Task<string> GenerateSKUAsync(Guid itemId)
    {
        var count = await _context.Set<ItemVariant>().CountAsync(x => x.ItemId == itemId) + 1;
        var shortId = itemId.ToString("N")[..6].ToUpper();
        return $"SKU-{shortId}-{count:D3}";
    }
}
