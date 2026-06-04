using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features.PurchaseRequisitions.Commands;
using uOrgHub.Procurement.Features.PurchaseRequisitions.Queries;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Procurement;

[Authorize]
public class PurchaseRequisitionsController : BaseController
{
    private readonly IMediator _mediator;
    public PurchaseRequisitionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Procurement.PurchaseRequisitions.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] PRStatus? status = null)
    {
        var result = await _mediator.Send(new GetPRsQuery(request, status));
        return Ok(ApiResponse<PagedResult<PRResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Procurement.PurchaseRequisitions.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPRByIdQuery(id));
        return Ok(ApiResponse<PRResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Procurement.PurchaseRequisitions.Create)]
    public async Task<IActionResult> Create([FromBody] CreatePRDto dto)
    {
        var result = await _mediator.Send(new CreatePRCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PRResponseDto>.Ok(result, "Purchase requisition created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Procurement.PurchaseRequisitions.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePRDto dto)
    {
        var result = await _mediator.Send(new UpdatePRCommand(id, dto));
        return Ok(ApiResponse<PRResponseDto>.Ok(result, "Purchase requisition updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Procurement.PurchaseRequisitions.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeletePRCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Purchase requisition deleted successfully."));
    }

    [HttpPost("{id:guid}/submit")]
    [RequireClaim(Claims.Procurement.PurchaseRequisitions.Edit)]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _mediator.Send(new SubmitPRCommand(id));
        return Ok(ApiResponse<PRResponseDto>.Ok(result, "Purchase requisition submitted for approval."));
    }

    [HttpPost("{id:guid}/approve")]
    [RequireClaim(Claims.Procurement.PurchaseRequisitions.Approve)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApprovePRCommand(id));
        return Ok(ApiResponse<PRResponseDto>.Ok(result, "Purchase requisition approved."));
    }

    [HttpPost("{id:guid}/reject")]
    [RequireClaim(Claims.Procurement.PurchaseRequisitions.Approve)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectPRDto dto)
    {
        var result = await _mediator.Send(new RejectPRCommand(id, dto));
        return Ok(ApiResponse<PRResponseDto>.Ok(result, "Purchase requisition rejected."));
    }
}
