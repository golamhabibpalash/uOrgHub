using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public interface IRequestForQuotationRepository : IBaseRepository<RequestForQuotation>
{
    Task<RequestForQuotation?> GetByIdWithItemsAsync(Guid id);
    Task<string> GenerateRFQNumberAsync();
}
