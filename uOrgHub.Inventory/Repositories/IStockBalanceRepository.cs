using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public interface IStockBalanceRepository : IBaseRepository<StockBalance>
{
    Task<StockBalance?> GetByVariantAndWarehouseAsync(Guid itemVariantId, Guid warehouseId);
    Task<StockBalance> GetOrCreateAsync(Guid itemVariantId, Guid warehouseId);
}
