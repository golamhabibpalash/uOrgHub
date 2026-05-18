using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.Projects.DTOs;
using uOrgHub.Projects.Features.RABills.Commands;
using uOrgHub.Projects.Features.RABills.Queries;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Projects;

[Authorize]
public class RABillsController : BaseController
{
    private readonly IMediator _mediator;
    public RABillsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request,
        [FromQuery] Guid? projectId = null, [FromQuery] RABillStatus? status = null)
    {
        var result = await _mediator.Send(new GetRABillsQuery(request, projectId, status));
        return Ok(ApiResponse<PagedResult<RABillResponseDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRABillByIdQuery(id));
        return Ok(ApiResponse<RABillResponseDto>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRABillDto dto)
    {
        var result = await _mediator.Send(new CreateRABillCommand(dto));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<RABillResponseDto>.Ok(result, "RA Bill created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRABillDto dto)
    {
        var result = await _mediator.Send(new UpdateRABillCommand(id, dto));
        return Ok(ApiResponse<RABillResponseDto>.Ok(result, "RA Bill updated successfully."));
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        var result = await _mediator.Send(new SubmitRABillCommand(id));
        return Ok(ApiResponse<RABillResponseDto>.Ok(result, "RA Bill submitted successfully."));
    }

    [HttpPost("{id:guid}/certify")]
    public async Task<IActionResult> Certify(Guid id, [FromBody] CertifyRABillDto dto)
    {
        var result = await _mediator.Send(new CertifyRABillCommand(id, dto));
        return Ok(ApiResponse<RABillResponseDto>.Ok(result, "RA Bill certified successfully."));
    }

    [HttpPost("{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid id)
    {
        var result = await _mediator.Send(new MarkRABillPaidCommand(id));
        return Ok(ApiResponse<RABillResponseDto>.Ok(result, "RA Bill marked as paid."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteRABillCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "RA Bill deleted successfully."));
    }
}
