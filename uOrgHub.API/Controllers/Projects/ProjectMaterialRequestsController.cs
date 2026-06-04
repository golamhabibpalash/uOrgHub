using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.MaterialRequests.Commands;
using uOrgHub.Projects.Features.MaterialRequests.Queries;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ProjectMaterialRequestsController : BaseController
{
    private readonly IMediator _mediator;
    public ProjectMaterialRequestsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.MaterialRequests.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? projectId = null)
    {
        var result = await _mediator.Send(new GetMaterialRequestsQuery(request, projectId));
        return Ok(ApiResponse<PagedResult<MaterialRequestResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.MaterialRequests.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetMaterialRequestByIdQuery(id));
        return Ok(ApiResponse<MaterialRequestResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.MaterialRequests.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMaterialRequestDto dto)
    {
        var result = await _mediator.Send(new CreateMaterialRequestCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<MaterialRequestResponseDto>.Ok(result, "Material request created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.MaterialRequests.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaterialRequestDto dto)
    {
        var result = await _mediator.Send(new UpdateMaterialRequestCommand(id, dto));
        return Ok(ApiResponse<MaterialRequestResponseDto>.Ok(result, "Material request updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.MaterialRequests.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteMaterialRequestCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Material request deleted successfully."));
    }

    [HttpPost("{id:guid}/submit")]
    [RequireClaim(Claims.Projects.MaterialRequests.Edit)]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _mediator.Send(new SubmitMaterialRequestCommand(id));
        return Ok(ApiResponse<MaterialRequestResponseDto>.Ok(result, "Material request submitted successfully."));
    }

    [HttpPost("{id:guid}/approve")]
    [RequireClaim(Claims.Projects.MaterialRequests.Approve)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveMaterialRequestDto dto)
    {
        var result = await _mediator.Send(new ApproveMaterialRequestCommand(id, dto));
        return Ok(ApiResponse<MaterialRequestResponseDto>.Ok(result, "Material request approved successfully."));
    }
}
