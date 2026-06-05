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
public class ItemVariantsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    public ItemVariantsController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet]
    [RequireClaim(Claims.Inventory.ItemVariants.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? itemId = null)
    {
        var result = await _mediator.Send(new GetItemVariantsQuery(request, itemId));
        return Ok(ApiResponse<PagedResult<ItemVariantResponseDto>>.Ok(result));
    }

    [HttpGet("export")]
    [RequireClaim(Claims.Inventory.ItemVariants.Export)]
    public async Task<IActionResult> Export([FromQuery] string format = "xlsx", [FromQuery] Guid? itemId = null)
    {
        var data = await _mediator.Send(new GetAllItemVariantsForExportQuery(itemId));
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, ItemVariantExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "ItemVariants"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Inventory.ItemVariants.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetItemVariantByIdQuery(id));
        return Ok(ApiResponse<ItemVariantResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Inventory.ItemVariants.Create)]
    public async Task<IActionResult> Create([FromBody] CreateItemVariantDto dto)
    {
        var result = await _mediator.Send(new CreateItemVariantCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ItemVariantResponseDto>.Ok(result, "Item variant created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Inventory.ItemVariants.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemVariantDto dto)
    {
        var result = await _mediator.Send(new UpdateItemVariantCommand(id, dto));
        return Ok(ApiResponse<ItemVariantResponseDto>.Ok(result, "Item variant updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Inventory.ItemVariants.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteItemVariantCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Item variant deleted successfully."));
    }
}
