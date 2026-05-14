using Microsoft.EntityFrameworkCore;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Inventory.Repositories;

public class WarehouseRepository : BaseRepository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<Warehouse> ApplySearch(IQueryable<Warehouse> query, string search)
        => query.Where(x => x.Name.Contains(search) || x.Code.Contains(search));

    protected override IQueryable<Warehouse> ApplyOrdering(IQueryable<Warehouse> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null)
        => await BaseQuery().AnyAsync(x => x.Code == code && (excludeId == null || x.Id != excludeId));
}
