using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Models;

namespace uOrgHub.Accounts.Services;

public interface IAccountingReportService
{
    Task<TrialBalanceResponseDto> GetTrialBalanceAsync(ReportFilterDto filter);
    Task<List<GeneralLedgerRowDto>> GetGeneralLedgerAsync(ReportFilterDto filter);
    Task<IncomeStatementDto> GetIncomeStatementAsync(ReportFilterDto filter);
    Task<BalanceSheetDto> GetBalanceSheetAsync(ReportFilterDto filter);
    Task<List<AccountLedgerRowDto>> GetAccountLedgerAsync(Guid accountId, DateTime? dateFrom, DateTime? dateTo);
    Task<List<AccountLedgerGroupDto>> GetAllAccountsLedgerAsync(DateTime? dateFrom, DateTime? dateTo);
    Task<DayBookReportDto> GetDayBookAsync(DayBookFilterDto filter, PaginationRequest request);
    Task<List<ChartOfAccountsReportRowDto>> GetChartOfAccountsReportAsync(ReportFilterDto filter);
    Task<PagedResult<JournalEntryReportRowDto>> GetJournalEntryReportAsync(ReportFilterDto filter, PaginationRequest request);
    Task<List<AccountGroupSummaryRowDto>> GetAccountGroupSummaryAsync(ReportFilterDto filter);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<AgingSummaryDto> GetARAgingReportAsync(DateTime asOfDate);
    Task<AgingSummaryDto> GetAPAgingReportAsync(DateTime asOfDate);
}
