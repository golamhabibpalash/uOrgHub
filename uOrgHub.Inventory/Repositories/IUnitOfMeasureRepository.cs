using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public interface IUnitOfMeasureRepository : IBaseRepository<UnitOfMeasure>
{
    Task<bool> AbbreviationExistsAsync(string abbreviation, Guid? excludeId = null);
}
