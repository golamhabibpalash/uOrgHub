using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Stock.Queries;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class StockBalancesController : BaseController
{
    private readonly IMediator _mediator;
    public StockBalancesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Inventory.StockBalances.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? warehouseId = null, [FromQuery] Guid? itemVariantId = null)
    {
        var result = await _mediator.Send(new GetStockBalancesQuery(request, warehouseId, itemVariantId));
        return Ok(ApiResponse<PagedResult<StockBalanceResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Inventory.StockBalances.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetStockBalanceByIdQuery(id));
        return Ok(ApiResponse<StockBalanceResponseDto>.Ok(result));
    }
}
