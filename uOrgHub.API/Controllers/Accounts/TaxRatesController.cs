using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.TaxRate;
using uOrgHub.Accounts.Features.TaxRate;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/tax-rates")]
public class TaxRatesController : BaseController
{
    private readonly IMediator _mediator;
    public TaxRatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Accounts.TaxRates.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetTaxRatesQuery(request));
        return Ok(ApiResponse<PagedResult<TaxRateResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.TaxRates.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTaxRateByIdQuery(id));
        return Ok(ApiResponse<TaxRateResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.TaxRates.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTaxRateDto dto)
    {
        var result = await _mediator.Send(new CreateTaxRateCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<TaxRateResponseDto>.Ok(result, "Tax rate created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.TaxRates.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRateDto dto)
    {
        var result = await _mediator.Send(new UpdateTaxRateCommand(id, dto));
        return Ok(ApiResponse<TaxRateResponseDto>.Ok(result, "Tax rate updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.TaxRates.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTaxRateCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Tax rate deleted successfully."));
    }
}
