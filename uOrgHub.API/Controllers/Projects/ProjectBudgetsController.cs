using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.ProjectBudgets.Commands;
using uOrgHub.Projects.Features.ProjectBudgets.Queries;
using uOrgHub.Projects.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ProjectBudgetsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public ProjectBudgetsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Projects.Budgets.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid projectId)
    {
        var result = await _mediator.Send(new GetProjectBudgetsQuery(projectId, request));
        return Ok(ApiResponse<PagedResult<ProjectBudgetResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Projects.Budgets.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllProjectBudgetsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, ProjectBudgetExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Budgets"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.Budgets.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectBudgetByIdQuery(id));
        return Ok(ApiResponse<ProjectBudgetResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.Budgets.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProjectBudgetDto dto)
    {
        var result = await _mediator.Send(new CreateProjectBudgetCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectBudgetResponseDto>.Ok(result, "Budget created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.Budgets.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectBudgetDto dto)
    {
        var result = await _mediator.Send(new UpdateProjectBudgetCommand(id, dto));
        return Ok(ApiResponse<ProjectBudgetResponseDto>.Ok(result, "Budget updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.Budgets.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProjectBudgetCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Budget deleted successfully."));
    }
}
