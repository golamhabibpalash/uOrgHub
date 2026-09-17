using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Repositories;
using uOrgHub.Shared.Services;

namespace uOrgHub.Procurement.Repositories;

public class VendorRepository : BaseRepository<Vendor>, IVendorRepository
{
    public VendorRepository(AppDbContext context) : base(context) { }

    protected override IQueryable<Vendor> ApplySearch(IQueryable<Vendor> query, string search)
        => query.Where(x => x.Name.Contains(search) || x.VendorCode.Contains(search) ||
                            (x.ContactPerson != null && x.ContactPerson.Contains(search)) ||
                            (x.Email != null && x.Email.Contains(search)));

    protected override IQueryable<Vendor> ApplyOrdering(IQueryable<Vendor> query, string? sortBy, bool descending)
        => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name);

    public Task<string> GenerateVendorCodeAsync() => VendorCodeGenerator.GenerateAsync(_context);
}
