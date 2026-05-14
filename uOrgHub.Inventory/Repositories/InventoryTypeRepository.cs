using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public class InventoryTypeRepository : BaseRepository<InventoryType>, IInventoryTypeRepository
{
    public InventoryTypeRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<InventoryType> ApplySearch(IQueryable<InventoryType> query, string search)
        => query.Where(x => x.Name.Contains(search) || x.Code.Contains(search));

    protected override IQueryable<InventoryType> ApplyOrdering(IQueryable<InventoryType> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId));
}
