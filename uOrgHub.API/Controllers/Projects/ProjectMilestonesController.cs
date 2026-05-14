using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.ProjectMilestones.Commands;
using uOrgHub.Projects.Features.ProjectMilestones.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ProjectMilestonesController : BaseController
{
    private readonly IMediator _mediator;
    public ProjectMilestonesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid projectId)
    {
        var result = await _mediator.Send(new GetProjectMilestonesListQuery(projectId, request));
        return Ok(ApiResponse<PagedResult<ProjectMilestoneResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectMilestoneByIdQuery(id));
        return Ok(ApiResponse<ProjectMilestoneResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectMilestoneDto dto)
    {
        var result = await _mediator.Send(new CreateProjectMilestoneCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectMilestoneResponseDto>.Ok(result, "Milestone created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectMilestoneDto dto)
    {
        var result = await _mediator.Send(new UpdateProjectMilestoneCommand(id, dto));
        return Ok(ApiResponse<ProjectMilestoneResponseDto>.Ok(result, "Milestone updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProjectMilestoneCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Milestone deleted successfully."));
    }
}
