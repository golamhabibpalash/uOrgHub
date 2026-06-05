using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features.CoreHR.Commands;
using uOrgHub.HR.Features.CoreHR.Queries;
using uOrgHub.HR.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/designations")]
public class DesignationController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public DesignationController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.HR.Designations.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? departmentId = null)
    {
        var result = await _mediator.Send(new GetDesignationsQuery(request, departmentId));
        return Ok(ApiResponse<PagedResult<DesignationResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.HR.Designations.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllDesignationsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, DesignationExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Designations"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("all")]
    [RequireClaim(Claims.HR.Designations.View)]
    public async Task<IActionResult> GetAllForDropdown()
    {
        var result = await _mediator.Send(new GetAllDesignationsQuery());
        return Ok(ApiResponse<List<DesignationResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.HR.Designations.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDesignationByIdQuery(id));
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result));
    }

    [HttpGet("{id:guid}/dependencies")]
    [RequireClaim(Claims.HR.Designations.Delete)]
    public async Task<IActionResult> GetDependencies(Guid id)
    {
        var result = await _mediator.Send(new GetDesignationDependenciesQuery(id));
        return Ok(ApiResponse<DesignationDependenciesDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.HR.Designations.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDesignationDto dto)
    {
        var result = await _mediator.Send(new CreateDesignationCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DesignationResponseDto>.Ok(result, "Designation created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.HR.Designations.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDesignationDto dto)
    {
        var result = await _mediator.Send(new UpdateDesignationCommand(id, dto));
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result, "Designation updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.HR.Designations.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDesignationCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Designation deleted successfully."));
    }
}
