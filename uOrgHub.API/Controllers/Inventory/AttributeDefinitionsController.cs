using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Features.Catalog.Commands;
using uOrgHub.Inventory.Features.Catalog.Queries;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Inventory;

[Authorize]
public class AttributeDefinitionsController : BaseController
{
    private readonly IMediator _mediator;
    public AttributeDefinitionsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequireClaim(Claims.Inventory.Attributes.View)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? categoryId = null)
    {
        var result = await _mediator.Send(new GetAttributeDefinitionsQuery(request, categoryId));
        return Ok(ApiResponse<PagedResult<AttributeDefinitionResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [RequireClaim(Claims.Inventory.Attributes.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAttributeDefinitionByIdQuery(id));
        return Ok(ApiResponse<AttributeDefinitionResponseDto>.Ok(result));
    }

    [HttpPost]
    [RequireClaim(Claims.Inventory.Attributes.Create)]
    public async Task<IActionResult> Create([FromBody] CreateAttributeDefinitionDto dto)
    {
        var result = await _mediator.Send(new CreateAttributeDefinitionCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AttributeDefinitionResponseDto>.Ok(result, "Attribute definition created successfully."));
    }

    [HttpPut("{id:guid}")]
    [RequireClaim(Claims.Inventory.Attributes.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttributeDefinitionDto dto)
    {
        var result = await _mediator.Send(new UpdateAttributeDefinitionCommand(id, dto));
        return Ok(ApiResponse<AttributeDefinitionResponseDto>.Ok(result, "Attribute definition updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [RequireClaim(Claims.Inventory.Attributes.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteAttributeDefinitionCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Attribute definition deleted successfully."));
    }
}
