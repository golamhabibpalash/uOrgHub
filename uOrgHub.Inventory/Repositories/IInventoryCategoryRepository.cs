using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public interface IInventoryCategoryRepository : IBaseRepository<InventoryCategory>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
}
