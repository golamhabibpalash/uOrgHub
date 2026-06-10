using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.Features.ChartOfAccount;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.Accounts.Services;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/chart-of-accounts")]
public class ChartOfAccountsController : BaseController
{
    private readonly IChartOfAccountService _service;
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public ChartOfAccountsController(IChartOfAccountService service, IMediator mediator, IExportService exportService)
    {
        _service = service;
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _service.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<ChartOfAccountResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllChartOfAccountsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, ChartOfAccountExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "ChartOfAccounts"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<ChartOfAccountResponseDto>.Ok(result));
    }

    [HttpGet("generate-code")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.View)]
    public async Task<IActionResult> GenerateCode([FromQuery] Guid accountGroupId)
    {
        var code = await _service.GenerateAccountCodeAsync(accountGroupId);
        return Ok(ApiResponse<string>.Ok(code));
    }

    [HttpGet("{id:guid}/ledger")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.View)]
    public async Task<IActionResult> GetLedger(Guid id)
    {
        var result = await _service.GetLedgerAsync(id);
        return Ok(ApiResponse<List<JournalEntryLineResponseDto>>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.Create)]
    public async Task<IActionResult> Create([FromBody] CreateChartOfAccountDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ChartOfAccountResponseDto>.Ok(result, "Chart of account created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChartOfAccountDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(ApiResponse<ChartOfAccountResponseDto>.Ok(result, "Chart of account updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.ChartOfAccounts.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.Ok("Deleted", "Chart of account deleted successfully."));
    }
}
