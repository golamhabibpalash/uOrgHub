using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features.PurchaseOrders.Commands;
using uOrgHub.Procurement.Features.PurchaseOrders.Queries;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Procurement;

[Authorize]
public class PurchaseOrdersController : BaseController
{
    private readonly IMediator _mediator;
    public PurchaseOrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] POStatus? status = null)
    {
        var result = await _mediator.Send(new GetPOsQuery(request, status));
        return Ok(ApiResponse<PagedResult<POResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPOByIdQuery(id));
        return Ok(ApiResponse<POResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var result = await _mediator.Send(new CreatePOCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<POResponseDto>.Ok(result, "Purchase order created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderDto dto)
    {
        var result = await _mediator.Send(new UpdatePOCommand(id, dto));
        return Ok(ApiResponse<POResponseDto>.Ok(result, "Purchase order updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeletePOCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Purchase order deleted successfully."));
    }

    [HttpPost("{id:guid}/send")]
    public async Task<IActionResult> Send(Guid id)
    {
        var result = await _mediator.Send(new SendPOCommand(id));
        return Ok(ApiResponse<POResponseDto>.Ok(result, "Purchase order sent to vendor."));
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _mediator.Send(new ConfirmPOCommand(id));
        return Ok(ApiResponse<POResponseDto>.Ok(result, "Purchase order confirmed."));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelPOCommand(id));
        return Ok(ApiResponse<POResponseDto>.Ok(result, "Purchase order cancelled."));
    }

    [HttpGet("{id:guid}/grns")]
    public async Task<IActionResult> GetGRNs(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetPOGRNsQuery(id, request));
        return Ok(ApiResponse<PagedResult<GRNResponseDto>>.Ok(result));
    }
}
