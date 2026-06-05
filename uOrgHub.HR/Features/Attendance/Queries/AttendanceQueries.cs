using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.HR.Features.Attendance.Queries;

public record GetAttendanceLogsQuery(PaginationRequest Request, Guid? EmployeeId = null, DateTime? FromDate = null, DateTime? ToDate = null)
    : IQuery<PagedResult<AttendanceLogResponseDto>>;
public record GetAllAttendanceLogsQuery : IQuery<List<AttendanceLogResponseDto>>;
public record GetWorkSchedulesQuery(PaginationRequest Request) : IQuery<PagedResult<WorkScheduleResponseDto>>;
public record GetAllWorkSchedulesQuery : IQuery<List<WorkScheduleResponseDto>>;
public record GetShiftsQuery(PaginationRequest Request, Guid? WorkScheduleId = null) : IQuery<PagedResult<ShiftResponseDto>>;
public record GetAllShiftsQuery : IQuery<List<ShiftResponseDto>>;

public class GetAttendanceLogsQueryHandler : IRequestHandler<GetAttendanceLogsQuery, PagedResult<AttendanceLogResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAttendanceLogsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<AttendanceLogResponseDto>> Handle(GetAttendanceLogsQuery request, CancellationToken ct)
    {
        var query = _context.Set<AttendanceLog>().Include(x => x.Employee).Where(x => !x.IsDeleted);
        if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId);
        if (request.FromDate.HasValue) query = query.Where(x => x.AttendanceDate >= request.FromDate.Value.Date);
        if (request.ToDate.HasValue) query = query.Where(x => x.AttendanceDate <= request.ToDate.Value.Date);

        var totalCount = await query.CountAsync(ct);
        var logMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["employeeName"] = "Employee.FirstName",
            ["attendanceDate"] = "AttendanceDate",
            ["checkIn"] = "CheckIn",
            ["checkOut"] = "CheckOut",
            ["workHours"] = "WorkHours",
        };
        query = query.ApplySorting(request.Request.SortBy ?? "AttendanceDate", request.Request.SortDescending, logMappings);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<AttendanceLogResponseDto>
        {
            Items = items.Select(e => new AttendanceLogResponseDto
            {
                Id = e.Id, EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? $"{e.Employee.FirstName} {e.Employee.LastName}" : string.Empty,
                AttendanceDate = e.AttendanceDate, CheckIn = e.CheckIn, CheckOut = e.CheckOut,
                WorkHours = e.WorkHours, OvertimeHours = e.OvertimeHours,
                Source = e.Source, Status = e.Status, Remarks = e.Remarks, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllAttendanceLogsQueryHandler : IRequestHandler<GetAllAttendanceLogsQuery, List<AttendanceLogResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllAttendanceLogsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<AttendanceLogResponseDto>> Handle(GetAllAttendanceLogsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<AttendanceLog>().Include(x => x.Employee)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.AttendanceDate)
            .Select(x => new AttendanceLogResponseDto
            {
                Id = x.Id, EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null ? $"{x.Employee.FirstName} {x.Employee.LastName}" : string.Empty,
                AttendanceDate = x.AttendanceDate, CheckIn = x.CheckIn, CheckOut = x.CheckOut,
                WorkHours = x.WorkHours, OvertimeHours = x.OvertimeHours,
                Source = x.Source, Status = x.Status, Remarks = x.Remarks, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetWorkSchedulesQueryHandler : IRequestHandler<GetWorkSchedulesQuery, PagedResult<WorkScheduleResponseDto>>
{
    private readonly AppDbContext _context;
    public GetWorkSchedulesQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<WorkScheduleResponseDto>> Handle(GetWorkSchedulesQuery request, CancellationToken ct)
    {
        var query = _context.Set<WorkSchedule>().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Request.Search))
            query = query.WhereSearch(request.Request.Search, x => x.Name);

        var totalCount = await query.CountAsync(ct);
        var wsMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "Name",
            ["description"] = "Description",
        };
        query = query.ApplySorting(request.Request.SortBy ?? "Name", request.Request.SortDescending, wsMappings);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<WorkScheduleResponseDto>
        {
            Items = items.Select(e => new WorkScheduleResponseDto
            {
                Id = e.Id, Name = e.Name, Description = e.Description,
                StartTime = e.StartTime, EndTime = e.EndTime, TotalHours = e.TotalHours,
                IsFlexible = e.IsFlexible, GracePeriodMinutes = e.GracePeriodMinutes,
                WorkingDaysPerWeek = e.WorkingDaysPerWeek, IsActive = e.IsActive, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllWorkSchedulesQueryHandler : IRequestHandler<GetAllWorkSchedulesQuery, List<WorkScheduleResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllWorkSchedulesQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<WorkScheduleResponseDto>> Handle(GetAllWorkSchedulesQuery request, CancellationToken ct)
    {
        var items = await _context.Set<WorkSchedule>()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new WorkScheduleResponseDto
            {
                Id = x.Id, Name = x.Name, Description = x.Description,
                StartTime = x.StartTime, EndTime = x.EndTime, TotalHours = x.TotalHours,
                IsFlexible = x.IsFlexible, GracePeriodMinutes = x.GracePeriodMinutes,
                WorkingDaysPerWeek = x.WorkingDaysPerWeek, IsActive = x.IsActive, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}

public class GetShiftsQueryHandler : IRequestHandler<GetShiftsQuery, PagedResult<ShiftResponseDto>>
{
    private readonly AppDbContext _context;
    public GetShiftsQueryHandler(AppDbContext context) => _context = context;

    public async Task<PagedResult<ShiftResponseDto>> Handle(GetShiftsQuery request, CancellationToken ct)
    {
        var query = _context.Set<Shift>().Include(x => x.WorkSchedule).Where(x => !x.IsDeleted);
        if (request.WorkScheduleId.HasValue)
            query = query.Where(x => x.WorkScheduleId == request.WorkScheduleId);

        var totalCount = await query.CountAsync(ct);
        var shiftMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "Name",
            ["startTime"] = "StartTime",
            ["endTime"] = "EndTime",
            ["workScheduleName"] = "WorkSchedule.Name",
        };
        query = query.ApplySorting(request.Request.SortBy ?? "Name", request.Request.SortDescending, shiftMappings);
        var items = await query
            .Skip((request.Request.Page - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize).ToListAsync(ct);

        return new PagedResult<ShiftResponseDto>
        {
            Items = items.Select(e => new ShiftResponseDto
            {
                Id = e.Id, Name = e.Name, Code = e.Code,
                WorkScheduleId = e.WorkScheduleId, WorkScheduleName = e.WorkSchedule?.Name ?? string.Empty,
                StartTime = e.StartTime, EndTime = e.EndTime,
                IsNightShift = e.IsNightShift, IsActive = e.IsActive, CreatedAt = e.CreatedAt
            }).ToList(),
            TotalCount = totalCount, Page = request.Request.Page, PageSize = request.Request.PageSize
        };
    }
}

public class GetAllShiftsQueryHandler : IRequestHandler<GetAllShiftsQuery, List<ShiftResponseDto>>
{
    private readonly AppDbContext _context;
    public GetAllShiftsQueryHandler(AppDbContext context) => _context = context;

    public async Task<List<ShiftResponseDto>> Handle(GetAllShiftsQuery request, CancellationToken ct)
    {
        var items = await _context.Set<Shift>().Include(x => x.WorkSchedule)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => new ShiftResponseDto
            {
                Id = x.Id, Name = x.Name, Code = x.Code,
                WorkScheduleId = x.WorkScheduleId, WorkScheduleName = x.WorkSchedule != null ? x.WorkSchedule.Name : string.Empty,
                StartTime = x.StartTime, EndTime = x.EndTime,
                IsNightShift = x.IsNightShift, IsActive = x.IsActive, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return items;
    }
}
