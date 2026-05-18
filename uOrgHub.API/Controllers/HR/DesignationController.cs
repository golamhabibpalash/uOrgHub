using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features.CoreHR.Commands;
using uOrgHub.HR.Features.CoreHR.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/designations")]
public class DesignationController : BaseController
{
    private readonly IMediator _mediator;

    public DesignationController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request, [FromQuery] Guid? departmentId = null)
    {
        var result = await _mediator.Send(new GetDesignationsQuery(request, departmentId));
        return Ok(ApiResponse<PagedResult<DesignationResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDesignationByIdQuery(id));
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDesignationDto dto)
    {
        var result = await _mediator.Send(new CreateDesignationCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DesignationResponseDto>.Ok(result, "Designation created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDesignationDto dto)
    {
        var result = await _mediator.Send(new UpdateDesignationCommand(id, dto));
        return Ok(ApiResponse<DesignationResponseDto>.Ok(result, "Designation updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteDesignationCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Designation deleted successfully."));
    }
}
