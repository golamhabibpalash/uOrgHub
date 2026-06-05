using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.ProjectExpenses.Commands;
using uOrgHub.Projects.Features.ProjectExpenses.Queries;
using uOrgHub.Projects.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ProjectExpensesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public ProjectExpensesController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Projects.Expenses.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? projectId = null)
    {
        var result = await _mediator.Send(new GetProjectExpensesListQuery(request, projectId));
        return Ok(ApiResponse<PagedResult<ProjectExpenseResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Projects.Expenses.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllProjectExpensesForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, ProjectExpenseExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Expenses"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.Expenses.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectExpenseByIdQuery(id));
        return Ok(ApiResponse<ProjectExpenseResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.Expenses.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProjectExpenseDto dto)
    {
        var result = await _mediator.Send(new CreateProjectExpenseCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectExpenseResponseDto>.Ok(result, "Expense created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.Expenses.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectExpenseDto dto)
    {
        var result = await _mediator.Send(new UpdateProjectExpenseCommand(id, dto));
        return Ok(ApiResponse<ProjectExpenseResponseDto>.Ok(result, "Expense updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.Expenses.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProjectExpenseCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Expense deleted successfully."));
    }

    [HttpPost("{id:guid}/approve")]
    [RequireClaim(Claims.Projects.Expenses.Approve)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveExpenseDto dto)
    {
        var result = await _mediator.Send(new ApproveProjectExpenseCommand(id, dto));
        return Ok(ApiResponse<ProjectExpenseResponseDto>.Ok(result, "Expense approved successfully."));
    }
}
