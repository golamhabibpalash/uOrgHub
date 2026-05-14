using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Services;

namespace uOrgHub.Accounts.Services;

public interface IChartOfAccountService : IBaseService<ChartOfAccountResponseDto, CreateChartOfAccountDto, UpdateChartOfAccountDto>
{
    Task<List<JournalEntryLineResponseDto>> GetLedgerAsync(Guid accountId);
}