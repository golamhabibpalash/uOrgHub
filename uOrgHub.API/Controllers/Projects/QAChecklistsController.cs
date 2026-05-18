using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.QAChecklists.Commands;
using uOrgHub.Projects.Features.QAChecklists.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class QAChecklistsController : BaseController
{
    private readonly IMediator _mediator;
    public QAChecklistsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] QAChecklistStatus? status = null)
    {
        var result = await _mediator.Send(new GetQAChecklistsQuery(request, projectId, status));
        return Ok(ApiResponse<PagedResult<QAChecklistResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetQAChecklistByIdQuery(id));
        return Ok(ApiResponse<QAChecklistResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQAChecklistDto dto)
    {
        var result = await _mediator.Send(new CreateQAChecklistCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<QAChecklistResponseDto>.Ok(result, "QA checklist created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQAChecklistDto dto)
    {
        var result = await _mediator.Send(new UpdateQAChecklistCommand(id, dto));
        return Ok(ApiResponse<QAChecklistResponseDto>.Ok(result, "QA checklist updated successfully."));
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitQAChecklistDto dto)
    {
        var result = await _mediator.Send(new SubmitQAChecklistCommand(id, dto));
        return Ok(ApiResponse<QAChecklistResponseDto>.Ok(result, "QA checklist submitted successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteQAChecklistCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "QA checklist deleted successfully."));
    }
}
