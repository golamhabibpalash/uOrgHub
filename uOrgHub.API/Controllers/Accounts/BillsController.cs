using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.AP;
using uOrgHub.Accounts.Features.AP;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/bills")]
public class BillsController : BaseController
{
    private readonly IMediator _mediator;
    public BillsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? vendorId, [FromQuery] BillStatus? status)
    {
        var result = await _mediator.Send(new GetBillsQuery(request, vendorId, status));
        return Ok(ApiResponse<PagedResult<BillResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBillByIdQuery(id));
        return Ok(ApiResponse<BillResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBillDto dto)
    {
        var result = await _mediator.Send(new CreateBillCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BillResponseDto>.Ok(result, "Bill created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBillDto dto)
    {
        var result = await _mediator.Send(new UpdateBillCommand(id, dto));
        return Ok(ApiResponse<BillResponseDto>.Ok(result, "Bill updated successfully."));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveBillCommand(id));
        return Ok(ApiResponse<BillResponseDto>.Ok(result, "Bill approved successfully."));
    }

    [HttpPost("{id:guid}/void")]
    public async Task<IActionResult> Void(Guid id)
    {
        var result = await _mediator.Send(new VoidBillCommand(id));
        return Ok(ApiResponse<BillResponseDto>.Ok(result, "Bill voided successfully."));
    }
}
