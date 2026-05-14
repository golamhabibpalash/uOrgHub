using Microsoft.EntityFrameworkCore;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public class PurchaseOrderRepository : BaseRepository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<PurchaseOrder> ApplySearch(IQueryable<PurchaseOrder> query, string search)
        => query.Where(x => x.PONumber.Contains(search));

    protected override IQueryable<PurchaseOrder> ApplyOrdering(IQueryable<PurchaseOrder> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.PODate) : query.OrderBy(x => x.PODate);

    public async Task<PurchaseOrder?> GetByIdWithItemsAsync(Guid id)
        => await BaseQuery()
            .Include(x => x.Items)
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<string> GeneratePONumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Set<PurchaseOrder>()
            .Where(x => x.PODate.Year == year).CountAsync() + 1;
        return $"PO-{year}-{count:D4}";
    }
}
