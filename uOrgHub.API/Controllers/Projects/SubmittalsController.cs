using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.Submittals.Commands;
using uOrgHub.Projects.Features.Submittals.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class SubmittalsController : BaseController
{
    private readonly IMediator _mediator;
    public SubmittalsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.Submittals.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] SubmittalStatus? status = null)
    {
        var result = await _mediator.Send(new GetSubmittalsQuery(request, projectId, status));
        return Ok(ApiResponse<PagedResult<SubmittalResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.Submittals.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetSubmittalByIdQuery(id));
        return Ok(ApiResponse<SubmittalResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.Submittals.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSubmittalDto dto)
    {
        var result = await _mediator.Send(new CreateSubmittalCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<SubmittalResponseDto>.Ok(result, "Submittal created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.Submittals.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubmittalDto dto)
    {
        var result = await _mediator.Send(new UpdateSubmittalCommand(id, dto));
        return Ok(ApiResponse<SubmittalResponseDto>.Ok(result, "Submittal updated successfully."));
    }

    [HttpPost("{id:guid}/review")]
    [RequireClaim(Claims.Projects.Submittals.Edit)]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewSubmittalDto dto)
    {
        var result = await _mediator.Send(new ReviewSubmittalCommand(id, dto));
        return Ok(ApiResponse<SubmittalResponseDto>.Ok(result, "Submittal reviewed successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.Submittals.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteSubmittalCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Submittal deleted successfully."));
    }
}
