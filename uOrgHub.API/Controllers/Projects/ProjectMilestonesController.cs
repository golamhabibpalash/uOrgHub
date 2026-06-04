using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.ProjectMilestones.Commands;
using uOrgHub.Projects.Features.ProjectMilestones.Queries;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ProjectMilestonesController : BaseController
{
    private readonly IMediator _mediator;
    public ProjectMilestonesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.Milestones.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid projectId)
    {
        var result = await _mediator.Send(new GetProjectMilestonesListQuery(projectId, request));
        return Ok(ApiResponse<PagedResult<ProjectMilestoneResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.Milestones.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectMilestoneByIdQuery(id));
        return Ok(ApiResponse<ProjectMilestoneResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.Milestones.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProjectMilestoneDto dto)
    {
        var result = await _mediator.Send(new CreateProjectMilestoneCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectMilestoneResponseDto>.Ok(result, "Milestone created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.Milestones.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectMilestoneDto dto)
    {
        var result = await _mediator.Send(new UpdateProjectMilestoneCommand(id, dto));
        return Ok(ApiResponse<ProjectMilestoneResponseDto>.Ok(result, "Milestone updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.Milestones.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProjectMilestoneCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Milestone deleted successfully."));
    }
}
