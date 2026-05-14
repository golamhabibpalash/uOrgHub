using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public interface IItemVariantRepository : IBaseRepository<ItemVariant>
{
    Task<List<ItemVariant>> GetByItemIdAsync(Guid itemId);
    Task<string> GenerateSKUAsync(Guid itemId);
}
