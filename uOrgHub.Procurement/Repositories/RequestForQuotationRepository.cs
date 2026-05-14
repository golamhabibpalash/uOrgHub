using Microsoft.EntityFrameworkCore;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public class RequestForQuotationRepository : BaseRepository<RequestForQuotation>, IRequestForQuotationRepository
{
    public RequestForQuotationRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<RequestForQuotation> ApplySearch(IQueryable<RequestForQuotation> query, string search)
        => query.Where(x => x.RFQNumber.Contains(search) || x.Title.Contains(search));

    protected override IQueryable<RequestForQuotation> ApplyOrdering(IQueryable<RequestForQuotation> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.RFQDate) : query.OrderBy(x => x.RFQDate);

    public async Task<RequestForQuotation?> GetByIdWithItemsAsync(Guid id)
        => await BaseQuery()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<string> GenerateRFQNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Set<RequestForQuotation>()
            .Where(x => x.RFQDate.Year == year).CountAsync() + 1;
        return $"RFQ-{year}-{count:D4}";
    }
}
