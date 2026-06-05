using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.Budget;
using uOrgHub.Accounts.Features.Budget;
using uOrgHub.Accounts.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/budgets")]
public class BudgetsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public BudgetsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Accounts.Budgets.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? fiscalYearId)
    {
        var result = await _mediator.Send(new GetBudgetsQuery(request, fiscalYearId));
        return Ok(ApiResponse<PagedResult<BudgetResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Accounts.Budgets.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllBudgetsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, BudgetExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Budgets"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.Budgets.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBudgetByIdQuery(id));
        return Ok(ApiResponse<BudgetResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.Budgets.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetDto dto)
    {
        var result = await _mediator.Send(new CreateBudgetCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BudgetResponseDto>.Ok(result, "Budget created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.Budgets.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBudgetDto dto)
    {
        var result = await _mediator.Send(new UpdateBudgetCommand(id, dto));
        return Ok(ApiResponse<BudgetResponseDto>.Ok(result, "Budget updated successfully."));
    }

    [HttpPost("{id:guid}/approve")]
    [RequireClaim(Claims.Accounts.Budgets.Edit)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveBudgetCommand(id));
        return Ok(ApiResponse<BudgetResponseDto>.Ok(result, "Budget approved successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.Budgets.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteBudgetCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Budget deleted successfully."));
    }
}
