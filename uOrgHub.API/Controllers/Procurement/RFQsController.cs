using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Procurement.DTOs;
using uOrgHub.Procurement.Features.RequestForQuotations.Commands;
using uOrgHub.Procurement.Features.RequestForQuotations.Queries;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Procurement;

[Authorize]
public class RFQsController : BaseController
{
    private readonly IMediator _mediator;
    public RFQsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Procurement.RFQs.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] RFQStatus? status = null)
    {
        var result = await _mediator.Send(new GetRFQsQuery(request, status));
        return Ok(ApiResponse<PagedResult<RFQResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Procurement.RFQs.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRFQByIdQuery(id));
        return Ok(ApiResponse<RFQResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Procurement.RFQs.Create)]
    public async Task<IActionResult> Create([FromBody] CreateRFQDto dto)
    {
        var result = await _mediator.Send(new CreateRFQCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<RFQResponseDto>.Ok(result, "RFQ created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Procurement.RFQs.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRFQDto dto)
    {
        var result = await _mediator.Send(new UpdateRFQCommand(id, dto));
        return Ok(ApiResponse<RFQResponseDto>.Ok(result, "RFQ updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Procurement.RFQs.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteRFQCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "RFQ deleted successfully."));
    }

    [HttpGet("{id:guid}/quotations")]
    [RequireClaim(Claims.Procurement.RFQs.View)]
    public async Task<IActionResult> GetQuotations(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetRFQQuotationsQuery(id, request));
        return Ok(ApiResponse<PagedResult<VendorQuotationResponseDto>>.Ok(result));
    }
}
