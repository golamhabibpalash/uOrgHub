using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public interface IAttributeDefinitionRepository : IBaseRepository<AttributeDefinition>
{
    Task<List<AttributeDefinition>> GetByCategoryAsync(Guid categoryId);
}
