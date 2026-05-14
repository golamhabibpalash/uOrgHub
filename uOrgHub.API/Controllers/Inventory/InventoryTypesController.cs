using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Catalog.Commands;
using uOrgHub.Inventory.Features.Catalog.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class InventoryTypesController : BaseController
{
    private readonly IMediator _mediator;
    public InventoryTypesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetInventoryTypesQuery(request));
        return Ok(ApiResponse<PagedResult<InventoryTypeResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetInventoryTypeByIdQuery(id));
        return Ok(ApiResponse<InventoryTypeResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryTypeDto dto)
    {
        var result = await _mediator.Send(new CreateInventoryTypeCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<InventoryTypeResponseDto>.Ok(result, "Inventory type created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryTypeDto dto)
    {
        var result = await _mediator.Send(new UpdateInventoryTypeCommand(id, dto));
        return Ok(ApiResponse<InventoryTypeResponseDto>.Ok(result, "Inventory type updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteInventoryTypeCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Inventory type deleted successfully."));
    }
}
