using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.TaxRate;
using uOrgHub.Accounts.Features.TaxRate;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
public class TaxRatesController : BaseController
{
    private readonly IMediator _mediator;
    public TaxRatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetTaxRatesQuery(request));
        return Ok(ApiResponse<PagedResult<TaxRateResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTaxRateByIdQuery(id));
        return Ok(ApiResponse<TaxRateResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaxRateDto dto)
    {
        var result = await _mediator.Send(new CreateTaxRateCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<TaxRateResponseDto>.Ok(result, "Tax rate created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRateDto dto)
    {
        var result = await _mediator.Send(new UpdateTaxRateCommand(id, dto));
        return Ok(ApiResponse<TaxRateResponseDto>.Ok(result, "Tax rate updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTaxRateCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Tax rate deleted successfully."));
    }
}
