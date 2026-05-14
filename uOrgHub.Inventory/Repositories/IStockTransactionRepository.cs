using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public interface IStockTransactionRepository : IBaseRepository<StockTransaction>
{
    Task<string> GenerateTransactionNumberAsync();
}
