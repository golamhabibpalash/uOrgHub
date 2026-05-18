using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.HR.DTOs.Leave;
using uOrgHub.HR.Features.Leave.Commands;
using uOrgHub.HR.Features.Leave.Queries;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/leave")]
public class LeaveController : BaseController
{
    private readonly IMediator _mediator;

    public LeaveController(IMediator mediator) => _mediator = mediator;

    [HttpGet("types")]
    public async Task<IActionResult> GetTypes([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetLeaveTypesQuery(request));
        return Ok(ApiResponse<PagedResult<LeaveTypeResponseDto>>.Ok(result));
    }

    [HttpPost("types")]
    public async Task<IActionResult> CreateType([FromBody] CreateLeaveTypeDto dto)
    {
        var result = await _mediator.Send(new CreateLeaveTypeCommand(dto));
        return Ok(ApiResponse<LeaveTypeResponseDto>.Ok(result, "Leave type created successfully."));
    }

    [HttpPut("types/{id:guid}")]
    public async Task<IActionResult> UpdateType(Guid id, [FromBody] UpdateLeaveTypeDto dto)
    {
        var result = await _mediator.Send(new UpdateLeaveTypeCommand(id, dto));
        return Ok(ApiResponse<LeaveTypeResponseDto>.Ok(result, "Leave type updated successfully."));
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null, [FromQuery] LeaveStatus? status = null)
    {
        var result = await _mediator.Send(new GetLeaveRequestsQuery(request, employeeId, status));
        return Ok(ApiResponse<PagedResult<LeaveRequestResponseDto>>.Ok(result));
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateLeaveRequestDto dto)
    {
        var result = await _mediator.Send(new CreateLeaveRequestCommand(dto));
        return Ok(ApiResponse<LeaveRequestResponseDto>.Ok(result, "Leave request submitted successfully."));
    }

    [HttpPost("requests/approve")]
    public async Task<IActionResult> ApproveRequest([FromBody] ApproveLeaveRequestDto dto)
    {
        var result = await _mediator.Send(new ApproveLeaveRequestCommand(dto));
        return Ok(ApiResponse<LeaveApprovalResponseDto>.Ok(result, "Leave request processed successfully."));
    }

    [HttpPut("requests/{id:guid}/cancel")]
    public async Task<IActionResult> CancelRequest(Guid id)
    {
        await _mediator.Send(new CancelLeaveRequestCommand(id));
        return Ok(ApiResponse<string>.Ok("Cancelled", "Leave request cancelled successfully."));
    }

    [HttpGet("balances/{employeeId:guid}")]
    public async Task<IActionResult> GetBalances(Guid employeeId, [FromQuery] int? year = null)
    {
        var result = await _mediator.Send(new GetLeaveBalancesQuery(employeeId, year));
        return Ok(ApiResponse<List<LeaveBalanceResponseDto>>.Ok(result));
    }
}
