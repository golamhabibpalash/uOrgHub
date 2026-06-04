using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.ProjectBudgets.Commands;
using uOrgHub.Projects.Features.ProjectBudgets.Queries;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ProjectBudgetsController : BaseController
{
    private readonly IMediator _mediator;
    public ProjectBudgetsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.Budgets.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid projectId)
    {
        var result = await _mediator.Send(new GetProjectBudgetsQuery(projectId, request));
        return Ok(ApiResponse<PagedResult<ProjectBudgetResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.Budgets.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectBudgetByIdQuery(id));
        return Ok(ApiResponse<ProjectBudgetResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.Budgets.Create)]
    public async Task<IActionResult> Create([FromBody] CreateProjectBudgetDto dto)
    {
        var result = await _mediator.Send(new CreateProjectBudgetCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectBudgetResponseDto>.Ok(result, "Budget created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.Budgets.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectBudgetDto dto)
    {
        var result = await _mediator.Send(new UpdateProjectBudgetCommand(id, dto));
        return Ok(ApiResponse<ProjectBudgetResponseDto>.Ok(result, "Budget updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.Budgets.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProjectBudgetCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Budget deleted successfully."));
    }
}
