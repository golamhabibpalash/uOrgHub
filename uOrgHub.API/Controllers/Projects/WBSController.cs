using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.WBS.Commands;
using uOrgHub.Projects.Features.WBS.Queries;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class WBSController : BaseController
{
    private readonly IMediator _mediator;
    public WBSController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.WBS.View)]
    public async Task<IActionResult> GetAll([FromQuery] Guid projectId, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetWBSItemsQuery(projectId, request));
        return Ok(ApiResponse<PagedResult<WBSResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.WBS.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetWBSByIdQuery(id));
        return Ok(ApiResponse<WBSResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.WBS.Create)]
    public async Task<IActionResult> Create([FromBody] CreateWBSDto dto)
    {
        var result = await _mediator.Send(new CreateWBSCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<WBSResponseDto>.Ok(result, "WBS item created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.WBS.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWBSDto dto)
    {
        var result = await _mediator.Send(new UpdateWBSCommand(id, dto));
        return Ok(ApiResponse<WBSResponseDto>.Ok(result, "WBS item updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.WBS.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteWBSCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "WBS item deleted successfully."));
    }
}
