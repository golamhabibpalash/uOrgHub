using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.Drawings.Commands;
using uOrgHub.Projects.Features.Drawings.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class DrawingsController : BaseController
{
    private readonly IMediator _mediator;
    public DrawingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] DrawingStatus? status = null,
        [FromQuery] DrawingDiscipline? discipline = null)
    {
        var result = await _mediator.Send(new GetDrawingsQuery(request, projectId, status, discipline));
        return Ok(ApiResponse<PagedResult<DrawingResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDrawingByIdQuery(id));
        return Ok(ApiResponse<DrawingResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDrawingDto dto)
    {
        var result = await _mediator.Send(new CreateDrawingCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DrawingResponseDto>.Ok(result, "Drawing created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDrawingDto dto)
    {
        var result = await _mediator.Send(new UpdateDrawingCommand(id, dto));
        return Ok(ApiResponse<DrawingResponseDto>.Ok(result, "Drawing updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDrawingCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Drawing deleted successfully."));
    }
}
