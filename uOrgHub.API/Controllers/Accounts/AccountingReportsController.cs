using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Accounts.Services;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/reports")]
public class AccountingReportsController : BaseController
{
    private readonly IAccountingReportService _reportService;

    public AccountingReportsController(IAccountingReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("trial-balance")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetTrialBalance([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetTrialBalanceAsync(filter);
        return Ok(ApiResponse<TrialBalanceResponseDto>.Ok(result));
    }

    [HttpGet("general-ledger")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetGeneralLedgerAsync(filter);
        return Ok(ApiResponse<List<GeneralLedgerRowDto>>.Ok(result));
    }

    [HttpGet("income-statement")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetIncomeStatementAsync(filter);
        return Ok(ApiResponse<IncomeStatementDto>.Ok(result));
    }

    [HttpGet("balance-sheet")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetBalanceSheetAsync(filter);
        return Ok(ApiResponse<BalanceSheetDto>.Ok(result));
    }

    [HttpGet("account-ledger/{id:guid}")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetAccountLedger(Guid id, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var result = await _reportService.GetAccountLedgerAsync(id, dateFrom, dateTo);
        return Ok(ApiResponse<List<AccountLedgerRowDto>>.Ok(result));
    }

    [HttpGet("day-book")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetDayBook([FromQuery] DateTime date)
    {
        var result = await _reportService.GetDayBookAsync(date);
        return Ok(ApiResponse<List<DayBookRowDto>>.Ok(result));
    }

    [HttpGet("chart-of-accounts")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetChartOfAccountsReport([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetChartOfAccountsReportAsync(filter);
        return Ok(ApiResponse<List<ChartOfAccountsReportRowDto>>.Ok(result));
    }

    [HttpGet("journal-entries")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetJournalEntryReport([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetJournalEntryReportAsync(filter);
        return Ok(ApiResponse<List<JournalEntryReportRowDto>>.Ok(result));
    }

    [HttpGet("account-group-summary")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetAccountGroupSummary([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetAccountGroupSummaryAsync(filter);
        return Ok(ApiResponse<List<AccountGroupSummaryRowDto>>.Ok(result));
    }

    [HttpGet("dashboard-summary")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var result = await _reportService.GetDashboardSummaryAsync();
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(result));
    }

    [HttpGet("ar-aging")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetARAging([FromQuery] DateTime? asOfDate)
    {
        var result = await _reportService.GetARAgingReportAsync(asOfDate ?? DateTime.UtcNow);
        return Ok(ApiResponse<AgingSummaryDto>.Ok(result));
    }

    [HttpGet("ap-aging")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetAPAging([FromQuery] DateTime? asOfDate)
    {
        var result = await _reportService.GetAPAgingReportAsync(asOfDate ?? DateTime.UtcNow);
        return Ok(ApiResponse<AgingSummaryDto>.Ok(result));
    }
}
