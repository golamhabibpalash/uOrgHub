using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Accounts.Reporting.Pdf;
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
    private readonly IChartOfAccountService _chartOfAccountService;

    public AccountingReportsController(IAccountingReportService reportService, IChartOfAccountService chartOfAccountService)
    {
        _reportService = reportService;
        _chartOfAccountService = chartOfAccountService;
    }

    private static string PdfFileName(string entityName) => $"{entityName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

    /// <summary>Journal Entry Report and Day Book are paginated (max page size 100) for the JSON
    /// screen, but a PDF needs the full, unbounded dataset — so the PDF endpoints page through
    /// the same query internally rather than exposing a second, duplicate "for export" query.</summary>
    private async Task<List<JournalEntryReportRowDto>> GetAllJournalEntryRowsAsync(ReportFilterDto filter)
    {
        var rows = new List<JournalEntryReportRowDto>();
        var page = 1;
        while (true)
        {
            var result = await _reportService.GetJournalEntryReportAsync(filter, new PaginationRequest { Page = page, PageSize = 100 });
            rows.AddRange(result.Items);
            if (!result.HasNext) break;
            page++;
        }
        return rows;
    }

    private async Task<(List<DayBookRowDto> Rows, decimal TotalDebit, decimal TotalCredit)> GetAllDayBookRowsAsync(DayBookFilterDto filter)
    {
        var rows = new List<DayBookRowDto>();
        var page = 1;
        DayBookReportDto result;
        do
        {
            result = await _reportService.GetDayBookAsync(filter, new PaginationRequest { Page = page, PageSize = 100 });
            rows.AddRange(result.Rows.Items);
            page++;
        } while (result.Rows.HasNext);
        return (rows, result.TotalDebit, result.TotalCredit);
    }

    [HttpGet("trial-balance")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetTrialBalance([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetTrialBalanceAsync(filter);
        return Ok(ApiResponse<TrialBalanceResponseDto>.Ok(result));
    }

    [HttpGet("trial-balance/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetTrialBalancePdf([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetTrialBalanceAsync(filter);
        var bytes = TrialBalancePdfDocument.Build(result, filter.DateFrom, filter.DateTo);
        return File(bytes, "application/pdf", PdfFileName("TrialBalance"));
    }

    [HttpGet("general-ledger")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetGeneralLedgerAsync(filter);
        return Ok(ApiResponse<List<GeneralLedgerRowDto>>.Ok(result));
    }

    [HttpGet("general-ledger/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetGeneralLedgerPdf([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetGeneralLedgerAsync(filter);
        var bytes = GeneralLedgerPdfDocument.Build(result, filter.DateFrom, filter.DateTo);
        return File(bytes, "application/pdf", PdfFileName("GeneralLedger"));
    }

    [HttpGet("income-statement")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetIncomeStatementAsync(filter);
        return Ok(ApiResponse<IncomeStatementDto>.Ok(result));
    }

    [HttpGet("income-statement/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetIncomeStatementPdf([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetIncomeStatementAsync(filter);
        var bytes = IncomeStatementPdfDocument.Build(result, filter.DateFrom, filter.DateTo);
        return File(bytes, "application/pdf", PdfFileName("IncomeStatement"));
    }

    [HttpGet("balance-sheet")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetBalanceSheetAsync(filter);
        return Ok(ApiResponse<BalanceSheetDto>.Ok(result));
    }

    [HttpGet("balance-sheet/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetBalanceSheetPdf([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetBalanceSheetAsync(filter);
        var bytes = BalanceSheetPdfDocument.Build(result, filter.DateFrom, filter.DateTo);
        return File(bytes, "application/pdf", PdfFileName("BalanceSheet"));
    }

    [HttpGet("account-ledger/{id:guid}")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetAccountLedger(Guid id, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var result = await _reportService.GetAccountLedgerAsync(id, dateFrom, dateTo);
        return Ok(ApiResponse<List<AccountLedgerRowDto>>.Ok(result));
    }

    [HttpGet("account-ledger/{id:guid}/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetAccountLedgerPdf(Guid id, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var account = await _chartOfAccountService.GetByIdAsync(id);
        var result = await _reportService.GetAccountLedgerAsync(id, dateFrom, dateTo);
        var bytes = AccountLedgerPdfDocument.BuildSingle(account.AccountCode, account.AccountName, result, dateFrom, dateTo);
        return File(bytes, "application/pdf", PdfFileName("AccountLedger"));
    }

    [HttpGet("account-ledger")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetAllAccountsLedger([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var result = await _reportService.GetAllAccountsLedgerAsync(dateFrom, dateTo);
        return Ok(ApiResponse<List<AccountLedgerGroupDto>>.Ok(result));
    }

    [HttpGet("account-ledger/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetAllAccountsLedgerPdf([FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo)
    {
        var result = await _reportService.GetAllAccountsLedgerAsync(dateFrom, dateTo);
        var bytes = AccountLedgerPdfDocument.BuildAll(result, dateFrom, dateTo);
        return File(bytes, "application/pdf", PdfFileName("AccountLedger_AllAccounts"));
    }

    [HttpGet("day-book")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetDayBook([FromQuery] DayBookFilterDto filter, [FromQuery] PaginationRequest request)
    {
        var result = await _reportService.GetDayBookAsync(filter, request);
        return Ok(ApiResponse<DayBookReportDto>.Ok(result));
    }

    [HttpGet("day-book/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetDayBookPdf([FromQuery] DayBookFilterDto filter)
    {
        var (rows, totalDebit, totalCredit) = await GetAllDayBookRowsAsync(filter);
        var bytes = DayBookPdfDocument.Build(rows, totalDebit, totalCredit, filter.DateFrom, filter.DateTo);
        return File(bytes, "application/pdf", PdfFileName("DayBook"));
    }

    [HttpGet("chart-of-accounts")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetChartOfAccountsReport([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetChartOfAccountsReportAsync(filter);
        return Ok(ApiResponse<List<ChartOfAccountsReportRowDto>>.Ok(result));
    }

    [HttpGet("chart-of-accounts/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetChartOfAccountsReportPdf([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetChartOfAccountsReportAsync(filter);
        var bytes = ChartOfAccountsReportPdfDocument.Build(result);
        return File(bytes, "application/pdf", PdfFileName("ChartOfAccountsReport"));
    }

    [HttpGet("journal-entries")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetJournalEntryReport([FromQuery] ReportFilterDto filter, [FromQuery] PaginationRequest request)
    {
        var result = await _reportService.GetJournalEntryReportAsync(filter, request);
        return Ok(ApiResponse<PagedResult<JournalEntryReportRowDto>>.Ok(result));
    }

    [HttpGet("journal-entries/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetJournalEntryReportPdf([FromQuery] ReportFilterDto filter)
    {
        var rows = await GetAllJournalEntryRowsAsync(filter);
        var bytes = JournalEntryReportPdfDocument.Build(rows, filter.DateFrom, filter.DateTo);
        return File(bytes, "application/pdf", PdfFileName("JournalEntries"));
    }

    [HttpGet("account-group-summary")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetAccountGroupSummary([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetAccountGroupSummaryAsync(filter);
        return Ok(ApiResponse<List<AccountGroupSummaryRowDto>>.Ok(result));
    }

    [HttpGet("account-group-summary/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetAccountGroupSummaryPdf([FromQuery] ReportFilterDto filter)
    {
        var result = await _reportService.GetAccountGroupSummaryAsync(filter);
        var bytes = AccountGroupSummaryPdfDocument.Build(result);
        return File(bytes, "application/pdf", PdfFileName("AccountGroupSummary"));
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

    [HttpGet("ar-aging/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetARAgingPdf([FromQuery] DateTime? asOfDate)
    {
        var effectiveDate = asOfDate ?? DateTime.UtcNow;
        var result = await _reportService.GetARAgingReportAsync(effectiveDate);
        var bytes = ARAgingPdfDocument.Build(result, effectiveDate);
        return File(bytes, "application/pdf", PdfFileName("ARAging"));
    }

    [HttpGet("ap-aging")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetAPAging([FromQuery] DateTime? asOfDate)
    {
        var result = await _reportService.GetAPAgingReportAsync(asOfDate ?? DateTime.UtcNow);
        return Ok(ApiResponse<AgingSummaryDto>.Ok(result));
    }

    [HttpGet("ap-aging/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetAPAgingPdf([FromQuery] DateTime? asOfDate)
    {
        var effectiveDate = asOfDate ?? DateTime.UtcNow;
        var result = await _reportService.GetAPAgingReportAsync(effectiveDate);
        var bytes = APAgingPdfDocument.Build(result, effectiveDate);
        return File(bytes, "application/pdf", PdfFileName("APAging"));
    }

    [HttpGet("receipts-payments")]
    [RequireClaim(Claims.Accounts.Reports.View)]
    public async Task<IActionResult> GetReceiptsPayments([FromQuery] ReceiptsPaymentsFilterDto filter)
    {
        var result = await _reportService.GetReceiptsPaymentsAsync(filter);
        return Ok(ApiResponse<ReceiptsPaymentsReportDto>.Ok(result));
    }

    [HttpGet("receipts-payments/pdf")]
    [RequireClaim(Claims.Accounts.Reports.Print)]
    public async Task<IActionResult> GetReceiptsPaymentsPdf([FromQuery] ReceiptsPaymentsFilterDto filter)
    {
        var result = await _reportService.GetReceiptsPaymentsAsync(filter);
        var bytes = ReceiptsPaymentsPdfDocument.Build(result, filter.DateFrom, filter.DateTo);
        return File(bytes, "application/pdf", PdfFileName("ReceiptsPayments"));
    }
}
