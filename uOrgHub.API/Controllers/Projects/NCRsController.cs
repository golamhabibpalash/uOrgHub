using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.NCRs.Commands;
using uOrgHub.Projects.Features.NCRs.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class NCRsController : BaseController
{
    private readonly IMediator _mediator;
    public NCRsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] NCRStatus? status = null,
        [FromQuery] NCRSeverity? severity = null)
    {
        var result = await _mediator.Send(new GetNCRsQuery(request, projectId, status, severity));
        return Ok(ApiResponse<PagedResult<NCRResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetNCRByIdQuery(id));
        return Ok(ApiResponse<NCRResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNCRDto dto)
    {
        var result = await _mediator.Send(new CreateNCRCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<NCRResponseDto>.Ok(result, "NCR created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNCRDto dto)
    {
        var result = await _mediator.Send(new UpdateNCRCommand(id, dto));
        return Ok(ApiResponse<NCRResponseDto>.Ok(result, "NCR updated successfully."));
    }

    [HttpPost("{id:guid}/verify")]
    public async Task<IActionResult> Verify(Guid id, [FromBody] VerifyNCRDto dto)
    {
        var result = await _mediator.Send(new VerifyNCRCommand(id, dto));
        return Ok(ApiResponse<NCRResponseDto>.Ok(result, "NCR verified successfully."));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id)
    {
        var result = await _mediator.Send(new CloseNCRCommand(id));
        return Ok(ApiResponse<NCRResponseDto>.Ok(result, "NCR closed successfully."));
    }

    [HttpPost("{id:guid}/void")]
    public async Task<IActionResult> Void(Guid id)
    {
        var result = await _mediator.Send(new VoidNCRCommand(id));
        return Ok(ApiResponse<NCRResponseDto>.Ok(result, "NCR voided successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteNCRCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "NCR deleted successfully."));
    }
}
