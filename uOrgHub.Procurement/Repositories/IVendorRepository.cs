using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public interface IVendorRepository : IBaseRepository<Vendor>
{
    Task<string> GenerateVendorCodeAsync();
}
