using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.RFIs.Commands;
using uOrgHub.Projects.Features.RFIs.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class RFIsController : BaseController
{
    private readonly IMediator _mediator;
    public RFIsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.RFIs.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] RFIStatus? status = null)
    {
        var result = await _mediator.Send(new GetRFIsQuery(request, projectId, status));
        return Ok(ApiResponse<PagedResult<RFIResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.RFIs.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRFIByIdQuery(id));
        return Ok(ApiResponse<RFIResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.RFIs.Create)]
    public async Task<IActionResult> Create([FromBody] CreateRFIDto dto)
    {
        var result = await _mediator.Send(new CreateRFICommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<RFIResponseDto>.Ok(result, "RFI created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.RFIs.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRFIDto dto)
    {
        var result = await _mediator.Send(new UpdateRFICommand(id, dto));
        return Ok(ApiResponse<RFIResponseDto>.Ok(result, "RFI updated successfully."));
    }

    [HttpPost("{id:guid}/respond")]
    [RequireClaim(Claims.Projects.RFIs.Edit)]
    public async Task<IActionResult> Respond(Guid id, [FromBody] RespondRFIDto dto)
    {
        var result = await _mediator.Send(new RespondRFICommand(id, dto));
        return Ok(ApiResponse<RFIResponseDto>.Ok(result, "RFI responded successfully."));
    }

    [HttpPost("{id:guid}/close")]
    [RequireClaim(Claims.Projects.RFIs.Edit)]
    public async Task<IActionResult> Close(Guid id)
    {
        var result = await _mediator.Send(new CloseRFICommand(id));
        return Ok(ApiResponse<RFIResponseDto>.Ok(result, "RFI closed successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.RFIs.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteRFICommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "RFI deleted successfully."));
    }
}
