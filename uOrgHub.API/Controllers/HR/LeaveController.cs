using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Auth.Services;
using uOrgHub.HR.DTOs.Leave;
using uOrgHub.HR.Features.Leave.Commands;
using uOrgHub.HR.Features.Leave.Queries;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.HR.Reporting.ExportColumns;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
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
    private readonly IPermissionService _permissionService;

    public LeaveController(IMediator mediator, AppDbContext context, IExportService exportService, IPermissionService permissionService)
    {
        _mediator = mediator;
        _context = context;
        _exportService = exportService;
        _permissionService = permissionService;
    }

    private async Task<Guid?> GetCurrentEmployeeIdAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return null;

        var user = await _context.Set<ApplicationUser>().FindAsync(userId);
        return user?.EmployeeId;
    }

    private async Task<bool> HasClaimAsync(string claim)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return false;

        return await _permissionService.HasClaimAsync(userId, claim);
    }

    // ── Leave Types (Admin/Config only) ──────────────────────────────

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

    [HttpGet("types/active")]
    [RequireAnyClaim(Claims.HR.LeaveTypes.View, Claims.Self.SubmitLeave)]
    public async Task<IActionResult> GetActiveTypes()
    {
        var result = await _mediator.Send(new GetAllLeaveTypesQuery());
        return Ok(ApiResponse<List<LeaveTypeResponseDto>>.Ok(result));
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

    // ── Leave Requests (Self-service + HR Admin) ────────────────────

    [HttpGet("requests")]
    [RequireAnyClaim(Claims.HR.LeaveRequests.View, Claims.Self.ViewLeave)]
    public async Task<IActionResult> GetRequests([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null, [FromQuery] LeaveStatus? status = null)
    {
        var isHrAdmin = await HasClaimAsync(Claims.HR.LeaveRequests.View);

        if (!isHrAdmin)
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();
            if (currentEmployeeId == null)
                return BadRequest(ApiResponse<PagedResult<LeaveRequestResponseDto>>.Fail("Current user is not linked to an employee."));

            // Force filter to own requests — ignore any client-supplied employeeId
            employeeId = currentEmployeeId;
        }

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
    [RequireAnyClaim(Claims.HR.LeaveRequests.Create, Claims.Self.SubmitLeave)]
    public async Task<IActionResult> CreateRequest([FromBody] CreateLeaveRequestDto dto)
    {
        var isHrAdmin = await HasClaimAsync(Claims.HR.LeaveRequests.Create);
        if (!isHrAdmin)
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();
            if (currentEmployeeId == null)
                return BadRequest(ApiResponse<LeaveRequestResponseDto>.Fail("Current user is not linked to an employee."));

            if (dto.EmployeeId != Guid.Empty && dto.EmployeeId != currentEmployeeId.Value)
                return BadRequest(ApiResponse<LeaveRequestResponseDto>.Fail("You can only submit leave requests for yourself."));

            dto.EmployeeId = currentEmployeeId.Value;
        }

        var givenName = User.FindFirstValue("given_name") ?? "";
        var familyName = User.FindFirstValue("family_name") ?? "";
        var userName = $"{givenName} {familyName}".Trim();
        var result = await _mediator.Send(new CreateLeaveRequestCommand(dto, userName));
        return Ok(ApiResponse<LeaveRequestResponseDto>.Ok(result, "Leave request submitted successfully."));
    }

    [HttpPut("requests/{id:guid}")]
    [RequireAnyClaim(Claims.HR.LeaveRequests.Edit, Claims.Self.SubmitLeave)]
    public async Task<IActionResult> UpdateRequest(Guid id, [FromBody] UpdateLeaveRequestDto dto)
    {
        var leaveRequest = await _context.Set<LeaveRequest>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (leaveRequest == null)
            return NotFound(ApiResponse<LeaveRequestResponseDto>.NotFound("Leave request not found."));

        if (leaveRequest.Status != LeaveStatus.Pending)
            return BadRequest(ApiResponse<LeaveRequestResponseDto>.Fail("Only pending leave requests can be edited."));

        var isHrAdmin = await HasClaimAsync(Claims.HR.LeaveRequests.Edit);
        if (!isHrAdmin)
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();
            if (currentEmployeeId == null)
                return BadRequest(ApiResponse<LeaveRequestResponseDto>.Fail("Current user is not linked to an employee."));

            if (leaveRequest.EmployeeId != currentEmployeeId.Value)
                return BadRequest(ApiResponse<LeaveRequestResponseDto>.Fail("You can only edit your own leave requests."));
        }

        var result = await _mediator.Send(new UpdateLeaveRequestCommand(id, dto));
        return Ok(ApiResponse<LeaveRequestResponseDto>.Ok(result, "Leave request updated successfully."));
    }

    [HttpDelete("requests/{id:guid}")]
    [RequireAnyClaim(Claims.HR.LeaveRequests.Delete, Claims.Self.SubmitLeave)]
    public async Task<IActionResult> DeleteRequest(Guid id)
    {
        var leaveRequest = await _context.Set<LeaveRequest>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (leaveRequest == null)
            return NotFound(ApiResponse<string>.NotFound("Leave request not found."));

        if (leaveRequest.Status != LeaveStatus.Pending)
            return BadRequest(ApiResponse<string>.Fail("Only pending leave requests can be deleted."));

        var isHrAdmin = await HasClaimAsync(Claims.HR.LeaveRequests.Delete);
        if (!isHrAdmin)
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();
            if (currentEmployeeId == null)
                return BadRequest(ApiResponse<string>.Fail("Current user is not linked to an employee."));

            if (leaveRequest.EmployeeId != currentEmployeeId.Value)
                return BadRequest(ApiResponse<string>.Fail("You can only delete your own leave requests."));
        }

        await _mediator.Send(new DeleteLeaveRequestCommand(id));
        return Ok(ApiResponse<string>.Ok("Deleted", "Leave request deleted successfully."));
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
    [RequireAnyClaim(Claims.HR.LeaveRequests.Edit, Claims.Self.SubmitLeave)]
    public async Task<IActionResult> CancelRequest(Guid id)
    {
        var isHrAdmin = await HasClaimAsync(Claims.HR.LeaveRequests.Edit);
        if (!isHrAdmin)
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();
            if (currentEmployeeId == null)
                return BadRequest(ApiResponse<string>.Fail("Current user is not linked to an employee."));

            var leaveRequest = await _context.Set<LeaveRequest>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (leaveRequest == null)
                return NotFound(ApiResponse<string>.NotFound("Leave request not found."));

            if (leaveRequest.EmployeeId != currentEmployeeId.Value)
                return BadRequest(ApiResponse<string>.Fail("You can only cancel your own leave requests."));
        }

        await _mediator.Send(new CancelLeaveRequestCommand(id));
        return Ok(ApiResponse<string>.Ok("Cancelled", "Leave request cancelled successfully."));
    }

    [HttpGet("balances/{employeeId:guid}")]
    [RequireAnyClaim(Claims.HR.LeaveRequests.View, Claims.Self.ViewLeave)]
    public async Task<IActionResult> GetBalances(Guid employeeId, [FromQuery] int? year = null)
    {
        var isHrAdmin = await HasClaimAsync(Claims.HR.LeaveRequests.View);
        if (!isHrAdmin)
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();
            if (currentEmployeeId == null)
                return BadRequest(ApiResponse<List<LeaveBalanceResponseDto>>.Fail("Current user is not linked to an employee."));

            if (employeeId != currentEmployeeId.Value)
                return BadRequest(ApiResponse<List<LeaveBalanceResponseDto>>.Fail("You can only view your own leave balances."));

            employeeId = currentEmployeeId.Value;
        }

        var result = await _mediator.Send(new GetLeaveBalancesQuery(employeeId, year));
        return Ok(ApiResponse<List<LeaveBalanceResponseDto>>.Ok(result));
    }
}
