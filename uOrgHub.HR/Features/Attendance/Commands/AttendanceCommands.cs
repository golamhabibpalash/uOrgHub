using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.HR.Features.Attendance.Commands;

public record CreateWorkScheduleCommand(CreateWorkScheduleDto Dto) : ICommand<WorkScheduleResponseDto>;
public record CreateShiftCommand(CreateShiftDto Dto) : ICommand<ShiftResponseDto>;
public record CreateEmployeeRosterCommand(CreateEmployeeRosterDto Dto) : ICommand<EmployeeRosterResponseDto>;
public record UpdateEmployeeRosterCommand(Guid Id, UpdateEmployeeRosterDto Dto) : ICommand<EmployeeRosterResponseDto>;
public record CreateAttendanceLogCommand(CreateAttendanceLogDto Dto) : ICommand<AttendanceLogResponseDto>;
public record UpdateAttendanceLogCommand(Guid Id, UpdateAttendanceLogDto Dto) : ICommand<AttendanceLogResponseDto>;

public class CreateWorkScheduleCommandHandler : IRequestHandler<CreateWorkScheduleCommand, WorkScheduleResponseDto>
{
    private readonly AppDbContext _context;

    public CreateWorkScheduleCommandHandler(AppDbContext context) => _context = context;

    public async Task<WorkScheduleResponseDto> Handle(CreateWorkScheduleCommand request, CancellationToken ct)
    {
        var entity = new WorkSchedule
        {
            Name = request.Dto.Name, Description = request.Dto.Description,
            StartTime = request.Dto.StartTime, EndTime = request.Dto.EndTime,
            TotalHours = request.Dto.TotalHours, IsFlexible = request.Dto.IsFlexible,
            GracePeriodMinutes = request.Dto.GracePeriodMinutes,
            WorkingDaysPerWeek = request.Dto.WorkingDaysPerWeek,
            IsActive = request.Dto.IsActive, CreatedAt = DateTime.UtcNow
        };
        _context.Set<WorkSchedule>().Add(entity);
        await _context.SaveChangesAsync(ct);

        return AttendanceMappingHelper.MapWsToDto(entity);
    }
}

public class CreateShiftCommandHandler : IRequestHandler<CreateShiftCommand, ShiftResponseDto>
{
    private readonly AppDbContext _context;

    public CreateShiftCommandHandler(AppDbContext context) => _context = context;

    public async Task<ShiftResponseDto> Handle(CreateShiftCommand request, CancellationToken ct)
    {
        if (request.Dto.Code != null)
        {
            var exists = await _context.Set<Shift>().AnyAsync(x => !x.IsDeleted && x.Code == request.Dto.Code, ct);
            if (exists) throw new AppException($"Shift code '{request.Dto.Code}' already exists.");
        }

        var entity = new Shift
        {
            Name = request.Dto.Name, Code = request.Dto.Code,
            WorkScheduleId = request.Dto.WorkScheduleId,
            StartTime = request.Dto.StartTime, EndTime = request.Dto.EndTime,
            IsNightShift = request.Dto.IsNightShift, IsActive = request.Dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<Shift>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var ws = await _context.Set<WorkSchedule>().FindAsync(entity.WorkScheduleId);
        return new ShiftResponseDto
        {
            Id = entity.Id, Name = entity.Name, Code = entity.Code,
            WorkScheduleId = entity.WorkScheduleId, WorkScheduleName = ws?.Name ?? string.Empty,
            StartTime = entity.StartTime, EndTime = entity.EndTime,
            IsNightShift = entity.IsNightShift, IsActive = entity.IsActive, CreatedAt = entity.CreatedAt
        };
    }
}

public class CreateEmployeeRosterCommandHandler : IRequestHandler<CreateEmployeeRosterCommand, EmployeeRosterResponseDto>
{
    private readonly AppDbContext _context;

    public CreateEmployeeRosterCommandHandler(AppDbContext context) => _context = context;

    public async Task<EmployeeRosterResponseDto> Handle(CreateEmployeeRosterCommand request, CancellationToken ct)
    {
        var entity = new EmployeeRoster
        {
            EmployeeId = request.Dto.EmployeeId, ShiftId = request.Dto.ShiftId,
            RosterDate = request.Dto.RosterDate.Date, IsOff = request.Dto.IsOff,
            Note = request.Dto.Note, CreatedAt = DateTime.UtcNow
        };
        _context.Set<EmployeeRoster>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var shift = await _context.Set<Shift>().FindAsync(entity.ShiftId);
        return new EmployeeRosterResponseDto
        {
            Id = entity.Id, EmployeeId = entity.EmployeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
            ShiftId = entity.ShiftId, ShiftName = shift?.Name ?? string.Empty,
            RosterDate = entity.RosterDate, IsOff = entity.IsOff, Note = entity.Note, CreatedAt = entity.CreatedAt
        };
    }
}

public class UpdateEmployeeRosterCommandHandler : IRequestHandler<UpdateEmployeeRosterCommand, EmployeeRosterResponseDto>
{
    private readonly AppDbContext _context;

    public UpdateEmployeeRosterCommandHandler(AppDbContext context) => _context = context;

    public async Task<EmployeeRosterResponseDto> Handle(UpdateEmployeeRosterCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<EmployeeRoster>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(EmployeeRoster), request.Id);

        entity.ShiftId = request.Dto.ShiftId;
        entity.IsOff = request.Dto.IsOff;
        entity.Note = request.Dto.Note;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<EmployeeRoster>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        var shift = await _context.Set<Shift>().FindAsync(entity.ShiftId);
        return new EmployeeRosterResponseDto
        {
            Id = entity.Id, EmployeeId = entity.EmployeeId,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
            ShiftId = entity.ShiftId, ShiftName = shift?.Name ?? string.Empty,
            RosterDate = entity.RosterDate, IsOff = entity.IsOff, Note = entity.Note, CreatedAt = entity.CreatedAt
        };
    }
}

public class CreateAttendanceLogCommandHandler : IRequestHandler<CreateAttendanceLogCommand, AttendanceLogResponseDto>
{
    private readonly AppDbContext _context;

    public CreateAttendanceLogCommandHandler(AppDbContext context) => _context = context;

    public async Task<AttendanceLogResponseDto> Handle(CreateAttendanceLogCommand request, CancellationToken ct)
    {
        var exists = await _context.Set<AttendanceLog>()
            .AnyAsync(x => !x.IsDeleted && x.EmployeeId == request.Dto.EmployeeId
                && x.AttendanceDate == request.Dto.AttendanceDate.Date, ct);
        if (exists) throw new AppException("Attendance log already exists for this employee on this date.");

        var entity = new AttendanceLog
        {
            EmployeeId = request.Dto.EmployeeId,
            AttendanceDate = request.Dto.AttendanceDate.Date,
            CheckIn = request.Dto.CheckIn, CheckOut = request.Dto.CheckOut,
            Source = request.Dto.Source, Status = request.Dto.Status,
            Remarks = request.Dto.Remarks, CreatedAt = DateTime.UtcNow
        };
        if (entity.CheckIn.HasValue && entity.CheckOut.HasValue)
            entity.WorkHours = (decimal)(entity.CheckOut.Value - entity.CheckIn.Value).TotalHours;

        _context.Set<AttendanceLog>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        return AttendanceMappingHelper.MapAttendanceToDto(entity, employee);
    }
}

public class UpdateAttendanceLogCommandHandler : IRequestHandler<UpdateAttendanceLogCommand, AttendanceLogResponseDto>
{
    private readonly AppDbContext _context;

    public UpdateAttendanceLogCommandHandler(AppDbContext context) => _context = context;

    public async Task<AttendanceLogResponseDto> Handle(UpdateAttendanceLogCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<AttendanceLog>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(AttendanceLog), request.Id);

        entity.CheckIn = request.Dto.CheckIn;
        entity.CheckOut = request.Dto.CheckOut;
        entity.Status = request.Dto.Status;
        entity.Remarks = request.Dto.Remarks;
        entity.IsManuallyEdited = true;
        entity.UpdatedAt = DateTime.UtcNow;

        if (entity.CheckIn.HasValue && entity.CheckOut.HasValue)
            entity.WorkHours = (decimal)(entity.CheckOut.Value - entity.CheckIn.Value).TotalHours;

        _context.Set<AttendanceLog>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(entity.EmployeeId);
        return AttendanceMappingHelper.MapAttendanceToDto(entity, employee);
    }
}

file static class AttendanceMappingHelper
{
    internal static WorkScheduleResponseDto MapWsToDto(WorkSchedule e) => new()
    {
        Id = e.Id, Name = e.Name, Description = e.Description,
        StartTime = e.StartTime, EndTime = e.EndTime, TotalHours = e.TotalHours,
        IsFlexible = e.IsFlexible, GracePeriodMinutes = e.GracePeriodMinutes,
        WorkingDaysPerWeek = e.WorkingDaysPerWeek, IsActive = e.IsActive, CreatedAt = e.CreatedAt
    };

    internal static AttendanceLogResponseDto MapAttendanceToDto(AttendanceLog e, Employee? employee) => new()
    {
        Id = e.Id, EmployeeId = e.EmployeeId,
        EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
        AttendanceDate = e.AttendanceDate, CheckIn = e.CheckIn, CheckOut = e.CheckOut,
        WorkHours = e.WorkHours, OvertimeHours = e.OvertimeHours,
        Source = e.Source, Status = e.Status, Remarks = e.Remarks, CreatedAt = e.CreatedAt
    };
}
