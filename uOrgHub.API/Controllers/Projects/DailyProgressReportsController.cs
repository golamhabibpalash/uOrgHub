using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.DPR.Commands;
using uOrgHub.Projects.Features.DPR.Queries;
using uOrgHub.Shared.Models;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class DailyProgressReportsController : BaseController
{
    private readonly IMediator _mediator;
    public DailyProgressReportsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Projects.DPRs.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? projectId = null)
    {
        var result = await _mediator.Send(new GetDPRsQuery(request, projectId));
        return Ok(ApiResponse<PagedResult<DPRResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Projects.DPRs.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDPRByIdQuery(id));
        return Ok(ApiResponse<DPRResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Projects.DPRs.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDPRDto dto)
    {
        var result = await _mediator.Send(new CreateDPRCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DPRResponseDto>.Ok(result, "DPR created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Projects.DPRs.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDPRDto dto)
    {
        var result = await _mediator.Send(new UpdateDPRCommand(id, dto));
        return Ok(ApiResponse<DPRResponseDto>.Ok(result, "DPR updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Projects.DPRs.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDPRCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "DPR deleted successfully."));
    }

    [HttpPost("{id:guid}/submit")]
    [RequireClaim(Claims.Projects.DPRs.Edit)]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _mediator.Send(new SubmitDPRCommand(id));
        return Ok(ApiResponse<DPRResponseDto>.Ok(result, "DPR submitted successfully."));
    }

    [HttpPost("{id:guid}/approve")]
    [RequireClaim(Claims.Projects.DPRs.Edit)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveDPRDto dto)
    {
        var result = await _mediator.Send(new ApproveDPRCommand(id, dto));
        return Ok(ApiResponse<DPRResponseDto>.Ok(result, "DPR approved successfully."));
    }
}
