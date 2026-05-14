using Microsoft.EntityFrameworkCore;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public class PurchaseRequisitionRepository : BaseRepository<PurchaseRequisition>, IPurchaseRequisitionRepository
{
    public PurchaseRequisitionRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<PurchaseRequisition> ApplySearch(IQueryable<PurchaseRequisition> query, string search)
        => query.Where(x => x.PRNumber.Contains(search) || (x.Purpose != null && x.Purpose.Contains(search)));

    protected override IQueryable<PurchaseRequisition> ApplyOrdering(IQueryable<PurchaseRequisition> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.PRDate) : query.OrderBy(x => x.PRDate);

    public async Task<PurchaseRequisition?> GetByIdWithItemsAsync(Guid id)
        => await BaseQuery()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<string> GeneratePRNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Set<PurchaseRequisition>()
            .Where(x => x.PRDate.Year == year).CountAsync() + 1;
        return $"PR-{year}-{count:D4}";
    }
}
