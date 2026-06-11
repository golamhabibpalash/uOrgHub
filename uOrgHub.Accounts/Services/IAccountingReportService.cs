using uOrgHub.Accounts.DTOs.Reports;

namespace uOrgHub.Accounts.Services;

public interface IAccountingReportService
{
    Task<TrialBalanceResponseDto> GetTrialBalanceAsync(ReportFilterDto filter);
    Task<List<GeneralLedgerRowDto>> GetGeneralLedgerAsync(ReportFilterDto filter);
    Task<IncomeStatementDto> GetIncomeStatementAsync(ReportFilterDto filter);
    Task<BalanceSheetDto> GetBalanceSheetAsync(ReportFilterDto filter);
    Task<List<AccountLedgerRowDto>> GetAccountLedgerAsync(Guid accountId, DateTime? dateFrom, DateTime? dateTo);
    Task<List<DayBookRowDto>> GetDayBookAsync(DateTime date);
    Task<List<ChartOfAccountsReportRowDto>> GetChartOfAccountsReportAsync(ReportFilterDto filter);
    Task<List<JournalEntryReportRowDto>> GetJournalEntryReportAsync(ReportFilterDto filter);
    Task<List<AccountGroupSummaryRowDto>> GetAccountGroupSummaryAsync(ReportFilterDto filter);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<AgingSummaryDto> GetARAgingReportAsync(DateTime asOfDate);
    Task<AgingSummaryDto> GetAPAgingReportAsync(DateTime asOfDate);
}
