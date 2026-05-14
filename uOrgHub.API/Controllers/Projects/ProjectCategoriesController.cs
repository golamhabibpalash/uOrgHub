using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.ProjectCategories.Commands;
using uOrgHub.Projects.Features.ProjectCategories.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class ProjectCategoriesController : BaseController
{
    private readonly IMediator _mediator;
    public ProjectCategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetProjectCategoriesQuery(request));
        return Ok(ApiResponse<PagedResult<ProjectCategoryResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectCategoryByIdQuery(id));
        return Ok(ApiResponse<ProjectCategoryResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectCategoryDto dto)
    {
        var result = await _mediator.Send(new CreateProjectCategoryCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectCategoryResponseDto>.Ok(result, "Project category created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectCategoryDto dto)
    {
        var result = await _mediator.Send(new UpdateProjectCategoryCommand(id, dto));
        return Ok(ApiResponse<ProjectCategoryResponseDto>.Ok(result, "Project category updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProjectCategoryCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Project category deleted successfully."));
    }
}
