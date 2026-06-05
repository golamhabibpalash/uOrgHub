using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Warehouse.Commands;
using uOrgHub.Inventory.Features.Warehouse.Queries;
using uOrgHub.Inventory.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class WarehousesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public WarehousesController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Inventory.Warehouses.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetWarehousesQuery(request));
        return Ok(ApiResponse<PagedResult<WarehouseResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Inventory.Warehouses.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllWarehousesForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, WarehouseExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Warehouses"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Inventory.Warehouses.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetWarehouseByIdQuery(id));
        return Ok(ApiResponse<WarehouseResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Inventory.Warehouses.Create)]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
    {
        var result = await _mediator.Send(new CreateWarehouseCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<WarehouseResponseDto>.Ok(result, "Warehouse created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Inventory.Warehouses.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto dto)
    {
        var result = await _mediator.Send(new UpdateWarehouseCommand(id, dto));
        return Ok(ApiResponse<WarehouseResponseDto>.Ok(result, "Warehouse updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Inventory.Warehouses.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteWarehouseCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Warehouse deleted successfully."));
    }
}
