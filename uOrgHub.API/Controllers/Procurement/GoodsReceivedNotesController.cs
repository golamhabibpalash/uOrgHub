using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features.GoodsReceivedNotes.Commands;
using uOrgHub.Procurement.Features.GoodsReceivedNotes.Queries;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Procurement;

[Authorize]
public class GoodsReceivedNotesController : BaseController
{
    private readonly IMediator _mediator;
    public GoodsReceivedNotesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] GRNStatus? status = null)
    {
        var result = await _mediator.Send(new GetGRNsQuery(request, status));
        return Ok(ApiResponse<PagedResult<GRNResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetGRNByIdQuery(id));
        return Ok(ApiResponse<GRNResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGRNDto dto)
    {
        var result = await _mediator.Send(new CreateGRNCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<GRNResponseDto>.Ok(result, "GRN created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGRNDto dto)
    {
        var result = await _mediator.Send(new UpdateGRNCommand(id, dto));
        return Ok(ApiResponse<GRNResponseDto>.Ok(result, "GRN updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteGRNCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "GRN deleted successfully."));
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _mediator.Send(new ConfirmGRNCommand(id));
        return Ok(ApiResponse<GRNResponseDto>.Ok(result, "GRN confirmed. Stock balances updated."));
    }
}
