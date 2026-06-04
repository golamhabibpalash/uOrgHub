using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.ResourceAllocations.Commands;
using uOrgHub.Projects.Features.ResourceAllocations.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ResourceAllocationsController : BaseController
{
    private readonly IMediator _mediator;
    public ResourceAllocationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.ResourceAllocations.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] ResourceType? resourceType = null,
        [FromQuery] ResourceAllocationStatus? status = null)
    {
        var result = await _mediator.Send(new GetResourceAllocationsQuery(request, projectId, resourceType, status));
        return Ok(ApiResponse<PagedResult<ResourceAllocationResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.ResourceAllocations.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetResourceAllocationByIdQuery(id));
        return Ok(ApiResponse<ResourceAllocationResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.ResourceAllocations.Create)]
    public async Task<IActionResult> Create([FromBody] CreateResourceAllocationDto dto)
    {
        var result = await _mediator.Send(new CreateResourceAllocationCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ResourceAllocationResponseDto>.Ok(result, "Resource allocation created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.ResourceAllocations.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResourceAllocationDto dto)
    {
        var result = await _mediator.Send(new UpdateResourceAllocationCommand(id, dto));
        return Ok(ApiResponse<ResourceAllocationResponseDto>.Ok(result, "Resource allocation updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.ResourceAllocations.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteResourceAllocationCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Resource allocation deleted successfully."));
    }
}
