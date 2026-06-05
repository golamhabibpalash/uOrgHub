using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.Drawings.Commands;
using uOrgHub.Projects.Features.Drawings.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Projects.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class DrawingsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public DrawingsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Projects.Drawings.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] DrawingStatus? status = null,
        [FromQuery] DrawingDiscipline? discipline = null)
    {
        var result = await _mediator.Send(new GetDrawingsQuery(request, projectId, status, discipline));
        return Ok(ApiResponse<PagedResult<DrawingResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Projects.Drawings.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllDrawingsForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, DrawingExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Drawings"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.Drawings.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDrawingByIdQuery(id));
        return Ok(ApiResponse<DrawingResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.Drawings.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDrawingDto dto)
    {
        var result = await _mediator.Send(new CreateDrawingCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DrawingResponseDto>.Ok(result, "Drawing created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.Drawings.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDrawingDto dto)
    {
        var result = await _mediator.Send(new UpdateDrawingCommand(id, dto));
        return Ok(ApiResponse<DrawingResponseDto>.Ok(result, "Drawing updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.Drawings.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDrawingCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Drawing deleted successfully."));
    }
}
