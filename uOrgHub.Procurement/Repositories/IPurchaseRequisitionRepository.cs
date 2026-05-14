using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public interface IPurchaseRequisitionRepository : IBaseRepository<PurchaseRequisition>
{
    Task<PurchaseRequisition?> GetByIdWithItemsAsync(Guid id);
    Task<string> GeneratePRNumberAsync();
}
