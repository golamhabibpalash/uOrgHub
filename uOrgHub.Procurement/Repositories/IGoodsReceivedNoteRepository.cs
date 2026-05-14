using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Procurement.Repositories;

public interface IGoodsReceivedNoteRepository : IBaseRepository<GoodsReceivedNote>
{
    Task<GoodsReceivedNote?> GetByIdWithItemsAsync(Guid id);
    Task<string> GenerateGRNNumberAsync();
}
