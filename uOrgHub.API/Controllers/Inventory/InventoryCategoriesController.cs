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
public class InventoryCategoriesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public InventoryCategoriesController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Inventory.Categories.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? typeId = null)
    {
        var result = await _mediator.Send(new GetInventoryCategoriesQuery(request, typeId));
        return Ok(ApiResponse<PagedResult<InventoryCategoryResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Inventory.Categories.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] Guid? typeId = null)
    {
        var data = await _mediator.Send(new GetAllInventoryCategoriesForExportQuery(typeId));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, InventoryCategoryExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "InventoryCategories"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Inventory.Categories.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetInventoryCategoryByIdQuery(id));
        return Ok(ApiResponse<InventoryCategoryResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Inventory.Categories.Create)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryCategoryDto dto)
    {
        var result = await _mediator.Send(new CreateInventoryCategoryCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<InventoryCategoryResponseDto>.Ok(result, "Category created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Inventory.Categories.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryCategoryDto dto)
    {
        var result = await _mediator.Send(new UpdateInventoryCategoryCommand(id, dto));
        return Ok(ApiResponse<InventoryCategoryResponseDto>.Ok(result, "Category updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Inventory.Categories.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteInventoryCategoryCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Category deleted successfully."));
    }
}
