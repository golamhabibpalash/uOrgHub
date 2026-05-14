using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public class StockBalanceRepository : BaseRepository<StockBalance>, IStockBalanceRepository
{
    public StockBalanceRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<StockBalance> ApplyOrdering(IQueryable<StockBalance> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.LastUpdated) : query.OrderBy(x => x.LastUpdated);

    public async Task<StockBalance?> GetByVariantAndWarehouseAsync(Guid itemVariantId, Guid warehouseId)
        => await BaseQuery()
            .FirstOrDefaultAsync(x => x.ItemVariantId == itemVariantId && x.WarehouseId == warehouseId);

    public async Task<StockBalance> GetOrCreateAsync(Guid itemVariantId, Guid warehouseId)
    {
        var existing = await GetByVariantAndWarehouseAsync(itemVariantId, warehouseId);
        if (existing != null) return existing;

        var newBalance = new StockBalance
        {
            ItemVariantId = itemVariantId,
            WarehouseId = warehouseId,
            QuantityOnHand = 0,
            QuantityReserved = 0,
            LastUpdated = DateTime.UtcNow
        };
        _context.Set<StockBalance>().Add(newBalance);
        await _context.SaveChangesAsync();
        return newBalance;
    }
}
