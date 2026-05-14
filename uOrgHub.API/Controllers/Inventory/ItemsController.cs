using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Items.Commands;
using uOrgHub.Inventory.Features.Items.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class ItemsController : BaseController
{
    private readonly IMediator _mediator;
    public ItemsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? categoryId = null, [FromQuery] Guid? typeId = null)
    {
        var result = await _mediator.Send(new GetItemsQuery(request, categoryId, typeId));
        return Ok(ApiResponse<PagedResult<ItemResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetItemByIdQuery(id));
        return Ok(ApiResponse<ItemResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
    {
        var result = await _mediator.Send(new CreateItemCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ItemResponseDto>.Ok(result, "Item created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemDto dto)
    {
        var result = await _mediator.Send(new UpdateItemCommand(id, dto));
        return Ok(ApiResponse<ItemResponseDto>.Ok(result, "Item updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteItemCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Item deleted successfully."));
    }

    [HttpGet("{id:guid}/variants")]
    public async Task<IActionResult> GetVariants(Guid id, [FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetItemVariantsQuery(request, id));
        return Ok(ApiResponse<PagedResult<ItemVariantResponseDto>>.Ok(result));
    }
}
