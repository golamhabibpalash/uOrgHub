using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.ProjectExpenses.Commands;
using uOrgHub.Projects.Features.ProjectExpenses.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ProjectExpensesController : BaseController
{
    private readonly IMediator _mediator;
    public ProjectExpensesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? projectId = null)
    {
        var result = await _mediator.Send(new GetProjectExpensesListQuery(request, projectId));
        return Ok(ApiResponse<PagedResult<ProjectExpenseResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectExpenseByIdQuery(id));
        return Ok(ApiResponse<ProjectExpenseResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectExpenseDto dto)
    {
        var result = await _mediator.Send(new CreateProjectExpenseCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectExpenseResponseDto>.Ok(result, "Expense created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectExpenseDto dto)
    {
        var result = await _mediator.Send(new UpdateProjectExpenseCommand(id, dto));
        return Ok(ApiResponse<ProjectExpenseResponseDto>.Ok(result, "Expense updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProjectExpenseCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Expense deleted successfully."));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveExpenseDto dto)
    {
        var result = await _mediator.Send(new ApproveProjectExpenseCommand(id, dto));
        return Ok(ApiResponse<ProjectExpenseResponseDto>.Ok(result, "Expense approved successfully."));
    }
}
