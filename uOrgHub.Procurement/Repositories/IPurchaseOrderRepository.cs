using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public interface IPurchaseOrderRepository : IBaseRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetByIdWithItemsAsync(Guid id);
    Task<string> GeneratePONumberAsync();
}
