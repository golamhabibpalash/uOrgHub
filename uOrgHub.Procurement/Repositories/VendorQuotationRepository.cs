using Microsoft.EntityFrameworkCore;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public class VendorQuotationRepository : BaseRepository<VendorQuotation>, IVendorQuotationRepository
{
    public VendorQuotationRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<VendorQuotation> ApplySearch(IQueryable<VendorQuotation> query, string search)
        => query.Where(x => x.QuotationNumber.Contains(search));

    protected override IQueryable<VendorQuotation> ApplyOrdering(IQueryable<VendorQuotation> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.QuotationDate) : query.OrderBy(x => x.QuotationDate);

    public async Task<VendorQuotation?> GetByIdWithItemsAsync(Guid id)
        => await BaseQuery()
            .Include(x => x.Items)
            .Include(x => x.Vendor)
            .Include(x => x.RequestForQuotation)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<string> GenerateQuotationNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Set<VendorQuotation>()
            .Where(x => x.QuotationDate.Year == year).CountAsync() + 1;
        return $"QTN-{year}-{count:D4}";
    }
}
