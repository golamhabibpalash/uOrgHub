using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Catalog.Commands;
using uOrgHub.Inventory.Features.Catalog.Queries;
using uOrgHub.Inventory.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class InventoryTypesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public InventoryTypesController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Inventory.Types.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetInventoryTypesQuery(request));
        return Ok(ApiResponse<PagedResult<InventoryTypeResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Inventory.Types.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllInventoryTypesForExportQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, InventoryTypeExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "InventoryTypes"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Inventory.Types.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetInventoryTypeByIdQuery(id));
        return Ok(ApiResponse<InventoryTypeResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Inventory.Types.Create)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryTypeDto dto)
    {
        var result = await _mediator.Send(new CreateInventoryTypeCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<InventoryTypeResponseDto>.Ok(result, "Inventory type created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Inventory.Types.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryTypeDto dto)
    {
        var result = await _mediator.Send(new UpdateInventoryTypeCommand(id, dto));
        return Ok(ApiResponse<InventoryTypeResponseDto>.Ok(result, "Inventory type updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Inventory.Types.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteInventoryTypeCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Inventory type deleted successfully."));
    }
}
