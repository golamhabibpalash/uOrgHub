using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features.VendorQuotations.Commands;
using uOrgHub.Procurement.Features.VendorQuotations.Queries;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Procurement;

[Authorize]
public class VendorQuotationsController : BaseController
{
    private readonly IMediator _mediator;
    public VendorQuotationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] QuotationStatus? status = null)
    {
        var result = await _mediator.Send(new GetQuotationsQuery(request, status));
        return Ok(ApiResponse<PagedResult<VendorQuotationResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetQuotationByIdQuery(id));
        return Ok(ApiResponse<VendorQuotationResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorQuotationDto dto)
    {
        var result = await _mediator.Send(new CreateVendorQuotationCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<VendorQuotationResponseDto>.Ok(result, "Vendor quotation created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorQuotationDto dto)
    {
        var result = await _mediator.Send(new UpdateVendorQuotationCommand(id, dto));
        return Ok(ApiResponse<VendorQuotationResponseDto>.Ok(result, "Vendor quotation updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteVendorQuotationCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Vendor quotation deleted successfully."));
    }
}
