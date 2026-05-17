using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.Features.AR;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/invoices")]
public class InvoicesController : BaseController
{
    private readonly IMediator _mediator;
    public InvoicesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? customerId, [FromQuery] InvoiceStatus? status)
    {
        var result = await _mediator.Send(new GetInvoicesQuery(request, customerId, status));
        return Ok(ApiResponse<PagedResult<InvoiceResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(id));
        return Ok(ApiResponse<InvoiceResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceDto dto)
    {
        var result = await _mediator.Send(new CreateInvoiceCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<InvoiceResponseDto>.Ok(result, "Invoice created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceDto dto)
    {
        var result = await _mediator.Send(new UpdateInvoiceCommand(id, dto));
        return Ok(ApiResponse<InvoiceResponseDto>.Ok(result, "Invoice updated successfully."));
    }

    [HttpPost("{id:guid}/post")]
    public async Task<IActionResult> Post(Guid id)
    {
        var result = await _mediator.Send(new PostInvoiceCommand(id));
        return Ok(ApiResponse<InvoiceResponseDto>.Ok(result, "Invoice posted successfully."));
    }

    [HttpPost("{id:guid}/void")]
    public async Task<IActionResult> Void(Guid id)
    {
        var result = await _mediator.Send(new VoidInvoiceCommand(id));
        return Ok(ApiResponse<InvoiceResponseDto>.Ok(result, "Invoice voided successfully."));
    }
}
