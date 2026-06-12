using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Repositories;

namespace uOrgHub.Accounts.Repositories;

public interface IChartOfAccountRepository : IBaseRepository<ChartOfAccount>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null);
    Task<bool> CustomCodeExistsAsync(string customCode, Guid? excludeId = null);
    Task<string> GetNextAccountCodeAsync(Guid accountGroupId);
    Task<AccountGroupType?> GetAccountGroupTypeAsync(Guid accountGroupId);
    Task<List<JournalEntryLine>> GetLedgerAsync(Guid accountId);
}