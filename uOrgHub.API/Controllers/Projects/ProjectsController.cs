using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.Projects.Commands;
using uOrgHub.Projects.Features.Projects.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ProjectsController : BaseController
{
    private readonly IMediator _mediator;
    public ProjectsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] ProjectStatus? status = null, [FromQuery] Guid? clientId = null)
    {
        var result = await _mediator.Send(new GetProjectsQuery(request, status, clientId));
        return Ok(ApiResponse<PagedResult<ProjectResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id));
        return Ok(ApiResponse<ProjectResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        var result = await _mediator.Send(new CreateProjectCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectResponseDto>.Ok(result, "Project created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var result = await _mediator.Send(new UpdateProjectCommand(id, dto));
        return Ok(ApiResponse<ProjectResponseDto>.Ok(result, "Project updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProjectCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Project deleted successfully."));
    }

    // --- Dashboard ---

    [HttpGet("{id:guid}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid id)
    {
        var result = await _mediator.Send(new GetProjectDashboardQuery(id));
        return Ok(ApiResponse<ProjectDashboardDto>.Ok(result));
    }

    // --- WBS Tree ---

    [HttpGet("{id:guid}/wbs")]
    public async Task<IActionResult> GetWBSTree(Guid id)
    {
        var result = await _mediator.Send(new GetProjectWBSTreeQuery(id));
        return Ok(ApiResponse<List<WBSResponseDto>>.Ok(result));
    }

    // --- Budget Summary ---

    [HttpGet("{id:guid}/budget-summary")]
    public async Task<IActionResult> GetBudgetSummary(Guid id)
    {
        var result = await _mediator.Send(new GetProjectBudgetSummaryQuery(id));
        return Ok(ApiResponse<ProjectBudgetSummaryDto>.Ok(result));
    }

    // --- Progress ---

    [HttpGet("{id:guid}/progress")]
    public async Task<IActionResult> GetProgress(Guid id)
    {
        var result = await _mediator.Send(new GetProjectProgressQuery(id));
        return Ok(ApiResponse<ProjectProgressDto>.Ok(result));
    }

    // --- Team ---

    [HttpGet("{id:guid}/team")]
    public async Task<IActionResult> GetTeam(Guid id)
    {
        var result = await _mediator.Send(new GetProjectTeamQuery(id));
        return Ok(ApiResponse<List<ProjectTeamResponseDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/team")]
    public async Task<IActionResult> AddTeamMember(Guid id, [FromBody] AddProjectTeamMemberDto dto)
    {
        var result = await _mediator.Send(new AddProjectTeamMemberCommand(id, dto));
        return Ok(ApiResponse<ProjectTeamResponseDto>.Ok(result, "Team member added successfully."));
    }

    [HttpDelete("{id:guid}/team/{employeeId:guid}")]
    public async Task<IActionResult> RemoveTeamMember(Guid id, Guid employeeId)
    {
        await _mediator.Send(new RemoveProjectTeamMemberCommand(id, employeeId));
        return Ok(ApiResponse<string>.Ok("Removed", "Team member removed successfully."));
    }

    // --- Milestones ---

    [HttpGet("{id:guid}/milestones")]
    public async Task<IActionResult> GetMilestones(Guid id)
    {
        var result = await _mediator.Send(new GetProjectMilestonesQuery(id));
        return Ok(ApiResponse<List<ProjectMilestoneResponseDto>>.Ok(result));
    }

    // --- DPRs ---

    [HttpGet("{id:guid}/dprs")]
    public async Task<IActionResult> GetDPRs(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetProjectDPRsQuery(id, request));
        return Ok(ApiResponse<PagedResult<DPRResponseDto>>.Ok(result));
    }

    // --- Expenses ---

    [HttpGet("{id:guid}/expenses")]
    public async Task<IActionResult> GetExpenses(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetProjectExpensesQuery(id, request));
        return Ok(ApiResponse<PagedResult<ProjectExpenseResponseDto>>.Ok(result));
    }
}
