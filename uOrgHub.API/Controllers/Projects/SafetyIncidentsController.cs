using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.SafetyIncidents.Commands;
using uOrgHub.Projects.Features.SafetyIncidents.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class SafetyIncidentsController : BaseController
{
    private readonly IMediator _mediator;
    public SafetyIncidentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.SafetyIncidents.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] SafetyIncidentSeverity? severity = null,
        [FromQuery] SafetyIncidentStatus? status = null)
    {
        var result = await _mediator.Send(new GetSafetyIncidentsQuery(request, projectId, severity, status));
        return Ok(ApiResponse<PagedResult<SafetyIncidentResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.SafetyIncidents.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetSafetyIncidentByIdQuery(id));
        return Ok(ApiResponse<SafetyIncidentResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.SafetyIncidents.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSafetyIncidentDto dto)
    {
        var result = await _mediator.Send(new CreateSafetyIncidentCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SafetyIncidentResponseDto>.Ok(result, "Safety incident reported successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.SafetyIncidents.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSafetyIncidentDto dto)
    {
        var result = await _mediator.Send(new UpdateSafetyIncidentCommand(id, dto));
        return Ok(ApiResponse<SafetyIncidentResponseDto>.Ok(result, "Safety incident updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.SafetyIncidents.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteSafetyIncidentCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Safety incident deleted successfully."));
    }
}
