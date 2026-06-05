using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.Auth.Authorization;
using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.HR.Features.Attendance.Commands;
using uOrgHub.HR.Features.Attendance.Queries;
using uOrgHub.HR.Reporting.ExportColumns;
using uOrgHub.Shared.Export;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.HR;

[Authorize]
[Route("api/v1/attendance")]
public class AttendanceController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;

    public AttendanceController(IMediator mediator, IExportService exportService)
    {
        _mediator = mediator;
        _exportService = exportService;
    }

    [HttpGet("work-schedules")]
    [RequireClaim(Claims.HR.WorkSchedules.View)]
    public async Task<IActionResult> GetWorkSchedules([FromQuery] PaginationRequest request)
    {
        var result = await _mediator.Send(new GetWorkSchedulesQuery(request));
        return Ok(ApiResponse<PagedResult<WorkScheduleResponseDto>>.Ok(result));
    }

    [HttpGet("work-schedules/export")]
    [RequireClaim(Claims.HR.WorkSchedules.Export)]
    public async Task<IActionResult> ExportWorkSchedules([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllWorkSchedulesQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, WorkScheduleExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Work Schedules"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("work-schedules")]
    [RequireClaim(Claims.HR.WorkSchedules.Create)]
    public async Task<IActionResult> CreateWorkSchedule([FromBody] CreateWorkScheduleDto dto)
    {
        var result = await _mediator.Send(new CreateWorkScheduleCommand(dto));
        return Ok(ApiResponse<WorkScheduleResponseDto>.Ok(result, "Work schedule created successfully."));
    }

    [HttpGet("shifts")]
    [RequireClaim(Claims.HR.Shifts.View)]
    public async Task<IActionResult> GetShifts([FromQuery] PaginationRequest request, [FromQuery] Guid? workScheduleId = null)
    {
        var result = await _mediator.Send(new GetShiftsQuery(request, workScheduleId));
        return Ok(ApiResponse<PagedResult<ShiftResponseDto>>.Ok(result));
    }

    [HttpGet("shifts/export")]
    [RequireClaim(Claims.HR.Shifts.Export)]
    public async Task<IActionResult> ExportShifts([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllShiftsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, ShiftExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Shifts"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("shifts")]
    [RequireClaim(Claims.HR.Shifts.Create)]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto dto)
    {
        var result = await _mediator.Send(new CreateShiftCommand(dto));
        return Ok(ApiResponse<ShiftResponseDto>.Ok(result, "Shift created successfully."));
    }

    [HttpPost("rosters")]
    [RequireClaim(Claims.HR.Shifts.Edit)]
    public async Task<IActionResult> CreateRoster([FromBody] CreateEmployeeRosterDto dto)
    {
        var result = await _mediator.Send(new CreateEmployeeRosterCommand(dto));
        return Ok(ApiResponse<EmployeeRosterResponseDto>.Ok(result, "Roster entry created successfully."));
    }

    [HttpPut("rosters/{id:guid}")]
    [RequireClaim(Claims.HR.Shifts.Edit)]
    public async Task<IActionResult> UpdateRoster(Guid id, [FromBody] UpdateEmployeeRosterDto dto)
    {
        var result = await _mediator.Send(new UpdateEmployeeRosterCommand(id, dto));
        return Ok(ApiResponse<EmployeeRosterResponseDto>.Ok(result, "Roster entry updated successfully."));
    }

    [HttpGet("logs")]
    [RequireClaim(Claims.HR.AttendanceLogs.View)]
    public async Task<IActionResult> GetLogs([FromQuery] PaginationRequest request, [FromQuery] Guid? employeeId = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var result = await _mediator.Send(new GetAttendanceLogsQuery(request, employeeId, fromDate, toDate));
        return Ok(ApiResponse<PagedResult<AttendanceLogResponseDto>>.Ok(result));
    }

    [HttpGet("logs/export")]
    [RequireClaim(Claims.HR.AttendanceLogs.Export)]
    public async Task<IActionResult> ExportLogs([FromQuery] string format = "xlsx")
    {
        var data = await _mediator.Send(new GetAllAttendanceLogsQuery());
        var fmt = format.ToLower() switch { "csv" => ExportFormat.Csv, _ => ExportFormat.Xlsx };
        var result = await _exportService.ExportAsync(data, AttendanceLogExportColumns.Get(), new ExportOptions
        {
            Format = fmt,
            EntityName = "Attendance Logs"
        });
        return File(result.Content, result.MimeType, result.FileName);
    }

    [HttpPost("logs")]
    [RequireClaim(Claims.HR.AttendanceLogs.Create)]
    public async Task<IActionResult> CreateLog([FromBody] CreateAttendanceLogDto dto)
    {
        var result = await _mediator.Send(new CreateAttendanceLogCommand(dto));
        return Ok(ApiResponse<AttendanceLogResponseDto>.Ok(result, "Attendance log created successfully."));
    }

    [HttpPut("logs/{id:guid}")]
    [RequireClaim(Claims.HR.AttendanceLogs.Edit)]
    public async Task<IActionResult> UpdateLog(Guid id, [FromBody] UpdateAttendanceLogDto dto)
    {
        var result = await _mediator.Send(new UpdateAttendanceLogCommand(id, dto));
        return Ok(ApiResponse<AttendanceLogResponseDto>.Ok(result, "Attendance log updated successfully."));
    }
}
