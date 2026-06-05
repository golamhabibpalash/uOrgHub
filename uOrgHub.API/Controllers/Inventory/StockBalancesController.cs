using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Stock.Queries;
using uOrgHub.Inventory.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class StockBalancesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public StockBalancesController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Inventory.StockBalances.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? warehouseId = null, [FromQuery] Guid? itemVariantId = null)
    {
        var result = await _mediator.Send(new GetStockBalancesQuery(request, warehouseId, itemVariantId));
        return Ok(ApiResponse<PagedResult<StockBalanceResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Inventory.StockBalances.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] Guid? warehouseId = null, [FromQuery] Guid? itemVariantId = null)
    {
        var data = await _mediator.Send(new GetAllStockBalancesForExportQuery(warehouseId, itemVariantId));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, StockBalanceExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "StockBalances"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Inventory.StockBalances.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetStockBalanceByIdQuery(id));
        return Ok(ApiResponse<StockBalanceResponseDto>.Ok(result));
    }
}
