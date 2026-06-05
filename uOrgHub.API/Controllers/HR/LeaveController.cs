using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.HR.DTOs.Leave;
using uOrgHub.HR.Features.Leave.Commands;
using uOrgHub.HR.Features.Leave.Queries;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.HR.Reporting.ExportColumns;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/leave")]
public class LeaveController : BaseController
{
    private readonly IMediator _mediator;
    private readonly AppDbContext _context;
    private readonly IExportService _exportService;

    public LeaveController(IMediator mediator, AppDbContext context, IExportService exportService)
    {
        _mediator = mediator;
        _context = context;
        _exportService = exportService;
    }

    [HttpGet("types")]
    [RequireClaim(Claims.HR.LeaveTypes.View)]
    public async Task<IActionResult> GetTypes([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetLeaveTypesQuery(request));
        return Ok(ApiResponse<PagedResult<LeaveTypeResponseDto>>.Ok(result));
    }

    [HttpGet("leave-types/export")]
    [RequireClaim(Claims.HR.LeaveTypes.Export)]
    public async Task<IActionResult> ExportLeaveTypes([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllLeaveTypesQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, LeaveTypeExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Leave Types"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("types")]
    [RequireClaim(Claims.HR.LeaveTypes.Create)]
    public async Task<IActionResult> CreateType([FromBody] CreateLeaveTypeDto dto)
    {
        var result = await _mediator.Send(new CreateLeaveTypeCommand(dto));
        return Ok(ApiResponse<LeaveTypeResponseDto>.Ok(result, "Leave type created successfully."));
    }

    [HttpPut("types/{id:guid}")]
    [RequireClaim(Claims.HR.LeaveTypes.Edit)]
    public async Task<IActionResult> UpdateType(Guid id, [FromBody] UpdateLeaveTypeDto dto)
    {
        var result = await _mediator.Send(new UpdateLeaveTypeCommand(id, dto));
        return Ok(ApiResponse<LeaveTypeResponseDto>.Ok(result, "Leave type updated successfully."));
    }

    [HttpGet("requests")]
    [RequireClaim(Claims.HR.LeaveRequests.View)]
    public async Task<IActionResult> GetRequests([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null, [FromQuery] LeaveStatus? status = null)
    {
        var result = await _mediator.Send(new GetLeaveRequestsQuery(request, employeeId, status));
        return Ok(ApiResponse<PagedResult<LeaveRequestResponseDto>>.Ok(result));
    }

    [HttpGet("leave-requests/export")]
    [RequireClaim(Claims.HR.LeaveRequests.Export)]
    public async Task<IActionResult> ExportLeaveRequests([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllLeaveRequestsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, LeaveRequestExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Leave Requests"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("requests")]
    [RequireClaim(Claims.HR.LeaveRequests.Create)]
    public async Task<IActionResult> CreateRequest([FromBody] CreateLeaveRequestDto dto)
    {
        var result = await _mediator.Send(new CreateLeaveRequestCommand(dto));
        return Ok(ApiResponse<LeaveRequestResponseDto>.Ok(result, "Leave request submitted successfully."));
    }

    [HttpPost("requests/approve")]
    [RequireClaim(Claims.HR.LeaveRequests.Approve)]
    public async Task<IActionResult> ApproveRequest([FromBody] ApproveLeaveRequestDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Set<ApplicationUser>().FindAsync(userId);
        if (user?.EmployeeId == null)
            return BadRequest(ApiResponse<LeaveApprovalResponseDto>.Fail("Current user is not linked to an employee."));

        dto.ApproverId = user.EmployeeId.Value;

        var leaveRequest = await _context.Set<LeaveRequest>().FindAsync(dto.LeaveRequestId);
        dto.ApprovalLevel = leaveRequest?.CurrentApprovalLevel ?? 1;

        var result = await _mediator.Send(new ApproveLeaveRequestCommand(dto));
        return Ok(ApiResponse<LeaveApprovalResponseDto>.Ok(result, "Leave request processed successfully."));
    }

    [HttpPut("requests/{id:guid}/cancel")]
    [RequireClaim(Claims.HR.LeaveRequests.Edit)]
    public async Task<IActionResult> CancelRequest(Guid id)
    {
        await _mediator.Send(new CancelLeaveRequestCommand(id));
        return Ok(ApiResponse<string>.Ok("Cancelled", "Leave request cancelled successfully."));
    }

    [HttpGet("balances/{employeeId:guid}")]
    [RequireClaim(Claims.HR.LeaveRequests.View)]
    public async Task<IActionResult> GetBalances(Guid employeeId, [FromQuery] int? year = null)
    {
        var result = await _mediator.Send(new GetLeaveBalancesQuery(employeeId, year));
        return Ok(ApiResponse<List<LeaveBalanceResponseDto>>.Ok(result));
    }
}
