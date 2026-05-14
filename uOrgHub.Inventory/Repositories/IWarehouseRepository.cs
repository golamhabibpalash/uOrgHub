using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public interface IWarehouseRepository : IBaseRepository<Warehouse>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
}
