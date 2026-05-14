using Microsoft.EntityFrameworkCore;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public class VendorRepository : BaseRepository<Vendor>, IVendorRepository
{
    public VendorRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<Vendor> ApplySearch(IQueryable<Vendor> query, string search)
        => query.Where(x => x.CompanyName.Contains(search) || x.VendorCode.Contains(search) ||
                            (x.ContactPerson != null && x.ContactPerson.Contains(search)) ||
                            (x.Email != null && x.Email.Contains(search)));

    protected override IQueryable<Vendor> ApplyOrdering(IQueryable<Vendor> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.CompanyName) : query.OrderBy(x => x.CompanyName);

    public async Task<string> GenerateVendorCodeAsync()
    {
        var count = await _context.Set<Vendor>().CountAsync() + 1;
        return $"VND-{count:D4}";
    }
}
