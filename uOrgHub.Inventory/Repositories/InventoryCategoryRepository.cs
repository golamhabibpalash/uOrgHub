using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public class InventoryCategoryRepository : BaseRepository<InventoryCategory>, IInventoryCategoryRepository
{
    public InventoryCategoryRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<InventoryCategory> ApplySearch(IQueryable<InventoryCategory> query, string search)
        => query.Where(x => x.Name.Contains(search) || x.Code.Contains(search));

    protected override IQueryable<InventoryCategory> ApplyOrdering(IQueryable<InventoryCategory> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId));
}
