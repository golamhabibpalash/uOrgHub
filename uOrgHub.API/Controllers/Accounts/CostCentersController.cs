using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Accounts.DTOs.CostCenter;
using uOrgHub.Accounts.Features.CostCenter;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Accounts;

[Authorize]
[Route("api/v1/accounts/cost-centers")]
public class CostCentersController : BaseController
{
    private readonly IMediator _mediator;
    public CostCentersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Accounts.CostCenters.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetCostCentersQuery(request));
        return Ok(ApiResponse<PagedResult<CostCenterResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Accounts.CostCenters.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCostCenterByIdQuery(id));
        return Ok(ApiResponse<CostCenterResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Accounts.CostCenters.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCostCenterDto dto)
    {
        var result = await _mediator.Send(new CreateCostCenterCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CostCenterResponseDto>.Ok(result, "Cost center created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Accounts.CostCenters.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCostCenterDto dto)
    {
        var result = await _mediator.Send(new UpdateCostCenterCommand(id, dto));
        return Ok(ApiResponse<CostCenterResponseDto>.Ok(result, "Cost center updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Accounts.CostCenters.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteCostCenterCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Cost center deleted successfully."));
    }
}
