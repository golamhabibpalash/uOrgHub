using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.BOQ.Commands;
using uOrgHub.Projects.Features.BOQ.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class BOQsController : BaseController
{
    private readonly IMediator _mediator;
    public BOQsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] BOQStatus? status = null)
    {
        var result = await _mediator.Send(new GetBOQsQuery(request, projectId, status));
        return Ok(ApiResponse<PagedResult<BOQResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBOQByIdQuery(id));
        return Ok(ApiResponse<BOQResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBOQDto dto)
    {
        var result = await _mediator.Send(new CreateBOQCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BOQResponseDto>.Ok(result, "BOQ created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBOQDto dto)
    {
        var result = await _mediator.Send(new UpdateBOQCommand(id, dto));
        return Ok(ApiResponse<BOQResponseDto>.Ok(result, "BOQ updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteBOQCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "BOQ deleted successfully."));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveBOQDto dto)
    {
        var result = await _mediator.Send(new ApproveBOQCommand(id, dto));
        return Ok(ApiResponse<BOQResponseDto>.Ok(result, "BOQ approved successfully."));
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] CreateBOQItemDto dto)
    {
        var result = await _mediator.Send(new AddBOQItemCommand(id, dto));
        return Ok(ApiResponse<BOQResponseDto>.Ok(result, "BOQ item added successfully."));
    }

    [HttpPut("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, Guid itemId, [FromBody] UpdateBOQItemDto dto)
    {
        var result = await _mediator.Send(new UpdateBOQItemCommand(id, itemId, dto));
        return Ok(ApiResponse<BOQResponseDto>.Ok(result, "BOQ item updated successfully."));
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id, Guid itemId)
    {
        var result = await _mediator.Send(new DeleteBOQItemCommand(id, itemId));
        return Ok(ApiResponse<BOQResponseDto>.Ok(result, "BOQ item deleted successfully."));
    }
}
