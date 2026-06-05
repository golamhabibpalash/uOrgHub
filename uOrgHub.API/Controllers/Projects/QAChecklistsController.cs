using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.QAChecklists.Commands;
using uOrgHub.Projects.Features.QAChecklists.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Projects.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class QAChecklistsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public QAChecklistsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Projects.QAChecklists.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] QAChecklistStatus? status = null)
    {
        var result = await _mediator.Send(new GetQAChecklistsQuery(request, projectId, status));
        return Ok(ApiResponse<PagedResult<QAChecklistResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Projects.QAChecklists.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllQAChecklistsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, QAChecklistExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "QA Checklists"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.QAChecklists.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetQAChecklistByIdQuery(id));
        return Ok(ApiResponse<QAChecklistResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.QAChecklists.Create)]
    public async Task<IActionResult> Create([FromBody] CreateQAChecklistDto dto)
    {
        var result = await _mediator.Send(new CreateQAChecklistCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<QAChecklistResponseDto>.Ok(result, "QA checklist created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.QAChecklists.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQAChecklistDto dto)
    {
        var result = await _mediator.Send(new UpdateQAChecklistCommand(id, dto));
        return Ok(ApiResponse<QAChecklistResponseDto>.Ok(result, "QA checklist updated successfully."));
    }

    [HttpPost("{id:guid}/submit")]
    [RequireClaim(Claims.Projects.QAChecklists.Edit)]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitQAChecklistDto dto)
    {
        var result = await _mediator.Send(new SubmitQAChecklistCommand(id, dto));
        return Ok(ApiResponse<QAChecklistResponseDto>.Ok(result, "QA checklist submitted successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.QAChecklists.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteQAChecklistCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "QA checklist deleted successfully."));
    }
}
