using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Items.Commands;
using uOrgHub.Inventory.Features.Items.Queries;
using uOrgHub.Inventory.Reporting.ExportColumns;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class ItemsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public ItemsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Inventory.Items.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? categoryId = null, [FromQuery] Guid? typeId = null)
    {
        var result = await _mediator.Send(new GetItemsQuery(request, categoryId, typeId));
        return Ok(ApiResponse<PagedResult<ItemResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Inventory.Items.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] Guid? categoryId = null, [FromQuery] Guid? typeId = null, [FromQuery] string? search = null)
    {
        var data = await _mediator.Send(new GetAllItemsQuery(categoryId, typeId, search));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, ItemExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Items"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Inventory.Items.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetItemByIdQuery(id));
        return Ok(ApiResponse<ItemResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Inventory.Items.Create)]
    public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
    {
        var result = await _mediator.Send(new CreateItemCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ItemResponseDto>.Ok(result, "Item created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Inventory.Items.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemDto dto)
    {
        var result = await _mediator.Send(new UpdateItemCommand(id, dto));
        return Ok(ApiResponse<ItemResponseDto>.Ok(result, "Item updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Inventory.Items.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteItemCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Item deleted successfully."));
    }

    [HttpGet("{id:guid}/variants")]
    [RequireClaim(Claims.Inventory.Items.View)]
    public async Task<IActionResult> GetVariants(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetItemVariantsQuery(request, id));
        return Ok(ApiResponse<PagedResult<ItemVariantResponseDto>>.Ok(result));
    }
}
