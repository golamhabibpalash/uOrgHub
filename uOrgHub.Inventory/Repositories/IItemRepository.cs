using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public interface IItemRepository : IBaseRepository<Item>
{
    Task<string> GenerateItemCodeAsync();
}
