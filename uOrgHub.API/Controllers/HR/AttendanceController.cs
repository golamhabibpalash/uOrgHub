using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.HR.Features.Attendance.Commands;
using uOrgHub.HR.Features.Attendance.Queries;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
public class AttendanceController : BaseController
{
    private readonly IMediator _mediator;

    public AttendanceController(IMediator mediator) => _mediator = mediator;

    [HttpGet("work-schedules")]
    public async Task<IActionResult> GetWorkSchedules([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetWorkSchedulesQuery(request));
        return Ok(ApiResponse<PagedResult<WorkScheduleResponseDto>>.Ok(result));
    }

    [HttpPost("work-schedules")]
    public async Task<IActionResult> CreateWorkSchedule([FromBody] CreateWorkScheduleDto dto)
    {
        var result = await _mediator.Send(new CreateWorkScheduleCommand(dto));
        return Ok(ApiResponse<WorkScheduleResponseDto>.Ok(result, "Work schedule created successfully."));
    }

    [HttpGet("shifts")]
    public async Task<IActionResult> GetShifts([FromQuery] PaginationRequest request, [FromQuery] Guid? workScheduleId = null)
    {
        var result = await _mediator.Send(new GetShiftsQuery(request, workScheduleId));
        return Ok(ApiResponse<PagedResult<ShiftResponseDto>>.Ok(result));
    }

    [HttpPost("shifts")]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto dto)
    {
        var result = await _mediator.Send(new CreateShiftCommand(dto));
        return Ok(ApiResponse<ShiftResponseDto>.Ok(result, "Shift created successfully."));
    }

    [HttpPost("rosters")]
    public async Task<IActionResult> CreateRoster([FromBody] CreateEmployeeRosterDto dto)
    {
        var result = await _mediator.Send(new CreateEmployeeRosterCommand(dto));
        return Ok(ApiResponse<EmployeeRosterResponseDto>.Ok(result, "Roster entry created successfully."));
    }

    [HttpPut("rosters/{id:guid}")]
    public async Task<IActionResult> UpdateRoster(Guid id, [FromBody] UpdateEmployeeRosterDto dto)
    {
        var result = await _mediator.Send(new UpdateEmployeeRosterCommand(id, dto));
        return Ok(ApiResponse<EmployeeRosterResponseDto>.Ok(result, "Roster entry updated successfully."));
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var result = await _mediator.Send(new GetAttendanceLogsQuery(request, employeeId, fromDate, toDate));
        return Ok(ApiResponse<PagedResult<AttendanceLogResponseDto>>.Ok(result));
    }

    [HttpPost("logs")]
    public async Task<IActionResult> CreateLog([FromBody] CreateAttendanceLogDto dto)
    {
        var result = await _mediator.Send(new CreateAttendanceLogCommand(dto));
        return Ok(ApiResponse<AttendanceLogResponseDto>.Ok(result, "Attendance log created successfully."));
    }

    [HttpPut("logs/{id:guid}")]
    public async Task<IActionResult> UpdateLog(Guid id, [FromBody] UpdateAttendanceLogDto dto)
    {
        var result = await _mediator.Send(new UpdateAttendanceLogCommand(id, dto));
        return Ok(ApiResponse<AttendanceLogResponseDto>.Ok(result, "Attendance log updated successfully."));
    }
}
