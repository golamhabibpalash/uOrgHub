using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.Payment;
using uOrgHub.Accounts.Features.Payment;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
public class PaymentsController : BaseController
{
    private readonly IMediator _mediator;
    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? customerId, [FromQuery] Guid? vendorId)
    {
        var result = await _mediator.Send(new GetPaymentsQuery(request, customerId, vendorId));
        return Ok(ApiResponse<PagedResult<PaymentResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery(id));
        return Ok(ApiResponse<PaymentResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
    {
        var result = await _mediator.Send(new CreatePaymentCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PaymentResponseDto>.Ok(result, "Payment recorded successfully."));
    }
}
