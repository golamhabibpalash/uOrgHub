using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Catalog.Commands;
using uOrgHub.Inventory.Features.Catalog.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class UnitsOfMeasureController : BaseController
{
    private readonly IMediator _mediator;
    public UnitsOfMeasureController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetUnitsOfMeasureQuery(request));
        return Ok(ApiResponse<PagedResult<UnitOfMeasureResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetUnitOfMeasureByIdQuery(id));
        return Ok(ApiResponse<UnitOfMeasureResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUnitOfMeasureDto dto)
    {
        var result = await _mediator.Send(new CreateUnitOfMeasureCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<UnitOfMeasureResponseDto>.Ok(result, "Unit of measure created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitOfMeasureDto dto)
    {
        var result = await _mediator.Send(new UpdateUnitOfMeasureCommand(id, dto));
        return Ok(ApiResponse<UnitOfMeasureResponseDto>.Ok(result, "Unit of measure updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteUnitOfMeasureCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Unit of measure deleted successfully."));
    }
}
