using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Stock.Commands;
using uOrgHub.Inventory.Features.Stock.Queries;
using uOrgHub.Inventory.Models.Enums;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class StockTransactionsController : BaseController
{
    private readonly IMediator _mediator;
    public StockTransactionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Inventory.StockTransactions.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? warehouseId = null, [FromQuery] Guid? itemVariantId = null, [FromQuery] StockTransactionStatus? status = null)
    {
        var result = await _mediator.Send(new GetStockTransactionsQuery(request, warehouseId, itemVariantId, status));
        return Ok(ApiResponse<PagedResult<StockTransactionResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Inventory.StockTransactions.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetStockTransactionByIdQuery(id));
        return Ok(ApiResponse<StockTransactionResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Inventory.StockTransactions.Create)]
    public async Task<IActionResult> Create([FromBody] CreateStockTransactionDto dto)
    {
        var result = await _mediator.Send(new CreateStockTransactionCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<StockTransactionResponseDto>.Ok(result, "Stock transaction created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Inventory.StockTransactions.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockTransactionDto dto)
    {
        var result = await _mediator.Send(new UpdateStockTransactionCommand(id, dto));
        return Ok(ApiResponse<StockTransactionResponseDto>.Ok(result, "Stock transaction updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Inventory.StockTransactions.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteStockTransactionCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Stock transaction deleted successfully."));
    }

    [HttpPost("{id:guid}/confirm")]
    [RequireClaim(Claims.Inventory.StockTransactions.Edit)]
    public async Task<IActionResult> Confirm(Guid id)
    {
        var result = await _mediator.Send(new ConfirmStockTransactionCommand(id));
        return Ok(ApiResponse<StockTransactionResponseDto>.Ok(result, "Stock transaction confirmed and stock balances updated."));
    }

    [HttpPost("{id:guid}/cancel")]
    [RequireClaim(Claims.Inventory.StockTransactions.Edit)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelStockTransactionCommand(id));
        return Ok(ApiResponse<StockTransactionResponseDto>.Ok(result, "Stock transaction cancelled."));
    }
}
