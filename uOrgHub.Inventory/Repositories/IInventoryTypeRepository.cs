using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public interface IInventoryTypeRepository : IBaseRepository<InventoryType>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
}
