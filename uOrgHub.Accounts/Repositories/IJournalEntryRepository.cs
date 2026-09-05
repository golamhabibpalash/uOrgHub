using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Models;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Accounts.Repositories;

public interface IJournalEntryRepository : IBaseRepository<JournalEntry>
{
    Task<bool> EntryNumberExistsAsync(string entryNumber, Guid? excludeId = null);
    Task<string> GenerateEntryNumberAsync();
    Task<PagedResult<JournalEntry>> GetAllAsync(PaginationRequest request, DateTime? dateFrom, DateTime? dateTo);
}