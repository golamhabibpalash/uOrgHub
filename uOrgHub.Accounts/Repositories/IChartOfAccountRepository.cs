using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Accounts.Repositories;

public interface IChartOfAccountRepository : IBaseRepository<ChartOfAccount>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
    Task<List<JournalEntryLine>> GetLedgerAsync(Guid accountId);
}