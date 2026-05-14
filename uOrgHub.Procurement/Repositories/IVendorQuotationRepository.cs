using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public interface IVendorQuotationRepository : IBaseRepository<VendorQuotation>
{
    Task<VendorQuotation?> GetByIdWithItemsAsync(Guid id);
    Task<string> GenerateQuotationNumberAsync();
}
