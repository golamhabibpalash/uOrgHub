using Microsoft.EntityFrameworkCore;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public class GoodsReceivedNoteRepository : BaseRepository<GoodsReceivedNote>, IGoodsReceivedNoteRepository
{
    public GoodsReceivedNoteRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<GoodsReceivedNote> ApplySearch(IQueryable<GoodsReceivedNote> query, string search)
        => query.Where(x => x.GRNNumber.Contains(search) || (x.InvoiceNumber != null && x.InvoiceNumber.Contains(search)));

    protected override IQueryable<GoodsReceivedNote> ApplyOrdering(IQueryable<GoodsReceivedNote> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.GRNDate) : query.OrderBy(x => x.GRNDate);

    public async Task<GoodsReceivedNote?> GetByIdWithItemsAsync(Guid id)
        => await BaseQuery()
            .Include(x => x.Items)
            .Include(x => x.PurchaseOrder)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<string> GenerateGRNNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Set<GoodsReceivedNote>()
            .Where(x => x.GRNDate.Year == year).CountAsync() + 1;
        return $"GRN-{year}-{count:D4}";
    }
}
