using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.HR.Features._Common;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Services;

namespace uOrgHub.HR.Features.Attendance.Commands;

public record CreateWorkScheduleCommand(CreateWorkScheduleDto Dto) : ICommand<WorkScheduleResponseDto>;
public record UpdateWorkScheduleCommand(Guid Id, UpdateWorkScheduleDto Dto) : ICommand<WorkScheduleResponseDto>;
public record CreateShiftCommand(CreateShiftDto Dto) : ICommand<ShiftResponseDto>;
public record UpdateShiftCommand(Guid Id, UpdateShiftDto Dto) : ICommand<ShiftResponseDto>;
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

public class UpdateWorkScheduleCommandHandler : IRequestHandler<UpdateWorkScheduleCommand, WorkScheduleResponseDto>
{
    private readonly AppDbContext _context;

    public UpdateWorkScheduleCommandHandler(AppDbContext context) => _context = context;

    public async Task<WorkScheduleResponseDto> Handle(UpdateWorkScheduleCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<WorkSchedule>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(WorkSchedule), request.Id);

        entity.Name = request.Dto.Name; entity.Description = request.Dto.Description;
        entity.StartTime = request.Dto.StartTime; entity.EndTime = request.Dto.EndTime;
        entity.TotalHours = request.Dto.TotalHours; entity.IsFlexible = request.Dto.IsFlexible;
        entity.GracePeriodMinutes = request.Dto.GracePeriodMinutes;
        entity.WorkingDaysPerWeek = request.Dto.WorkingDaysPerWeek;
        entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

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
        var ws = await _context.Set<WorkSchedule>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Dto.WorkScheduleId, ct)
            ?? throw new NotFoundException(nameof(WorkSchedule), request.Dto.WorkScheduleId);

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

        return AttendanceMappingHelper.MapShiftToDto(entity, ws);
    }
}

public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand, ShiftResponseDto>
{
    private readonly AppDbContext _context;

    public UpdateShiftCommandHandler(AppDbContext context) => _context = context;

    public async Task<ShiftResponseDto> Handle(UpdateShiftCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<Shift>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(Shift), request.Id);

        if (request.Dto.Code != null)
        {
            var exists = await _context.Set<Shift>()
                .AnyAsync(x => !x.IsDeleted && x.Id != request.Id && x.Code == request.Dto.Code, ct);
            if (exists) throw new AppException($"Shift code '{request.Dto.Code}' already exists.");
        }
        var ws = await _context.Set<WorkSchedule>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Dto.WorkScheduleId, ct)
            ?? throw new NotFoundException(nameof(WorkSchedule), request.Dto.WorkScheduleId);

        entity.Name = request.Dto.Name; entity.Code = request.Dto.Code;
        entity.WorkScheduleId = request.Dto.WorkScheduleId;
        entity.StartTime = request.Dto.StartTime; entity.EndTime = request.Dto.EndTime;
        entity.IsNightShift = request.Dto.IsNightShift; entity.IsActive = request.Dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return AttendanceMappingHelper.MapShiftToDto(entity, ws);
    }
}

public class CreateEmployeeRosterCommandHandler : IRequestHandler<CreateEmployeeRosterCommand, EmployeeRosterResponseDto>
{
    private readonly AppDbContext _context;

    public CreateEmployeeRosterCommandHandler(AppDbContext context) => _context = context;

    public async Task<EmployeeRosterResponseDto> Handle(CreateEmployeeRosterCommand request, CancellationToken ct)
    {
        var rosterDate = request.Dto.RosterDate.Date;
        var employee = await _context.Set<Employee>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Dto.EmployeeId, ct)
            ?? throw new NotFoundException(nameof(Employee), request.Dto.EmployeeId);
        var shift = await _context.Set<Shift>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Dto.ShiftId, ct)
            ?? throw new NotFoundException(nameof(Shift), request.Dto.ShiftId);

        // The table has a unique (EmployeeId, RosterDate) index; check first for a readable error.
        var exists = await _context.Set<EmployeeRoster>()
            .AnyAsync(x => x.EmployeeId == request.Dto.EmployeeId && x.RosterDate == rosterDate, ct);
        if (exists)
            throw new AppException($"{employee.FirstName} {employee.LastName} is already rostered on {rosterDate:dd MMM yyyy}. Edit that entry instead.");

        var entity = new EmployeeRoster
        {
            EmployeeId = request.Dto.EmployeeId, ShiftId = request.Dto.ShiftId,
            RosterDate = rosterDate, IsOff = request.Dto.IsOff,
            Note = request.Dto.Note, CreatedAt = DateTime.UtcNow
        };
        _context.Set<EmployeeRoster>().Add(entity);
        await _context.SaveChangesAsync(ct);

        return AttendanceMappingHelper.MapRosterToDto(entity, employee, shift);
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
        var shift = await _context.Set<Shift>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Dto.ShiftId, ct)
            ?? throw new NotFoundException(nameof(Shift), request.Dto.ShiftId);

        entity.ShiftId = request.Dto.ShiftId;
        entity.IsOff = request.Dto.IsOff;
        entity.Note = request.Dto.Note;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<EmployeeRoster>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(new object[] { entity.EmployeeId }, ct);
        return AttendanceMappingHelper.MapRosterToDto(entity, employee, shift);
    }
}

public class CreateAttendanceLogCommandHandler : IRequestHandler<CreateAttendanceLogCommand, AttendanceLogResponseDto>
{
    private readonly AppDbContext _context;
    private readonly ICurrentCompanyAccessor _companyAccessor;

    public CreateAttendanceLogCommandHandler(AppDbContext context, ICurrentCompanyAccessor companyAccessor)
    {
        _context = context;
        _companyAccessor = companyAccessor;
    }

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
        var roster = await AttendanceRosterLookup.FindAsync(_context, entity.EmployeeId, entity.AttendanceDate, ct);
        var timeZone = await AttendanceRosterLookup.CompanyTimeZoneAsync(_context, _companyAccessor, ct);
        AttendanceShiftEvaluator.Apply(entity, roster, timeZone, applyLateRule: true);

        _context.Set<AttendanceLog>().Add(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(new object[] { entity.EmployeeId }, ct);
        return AttendanceMappingHelper.MapAttendanceToDto(entity, employee, roster?.Shift);
    }
}

public class UpdateAttendanceLogCommandHandler : IRequestHandler<UpdateAttendanceLogCommand, AttendanceLogResponseDto>
{
    private readonly AppDbContext _context;
    private readonly ICurrentCompanyAccessor _companyAccessor;

    public UpdateAttendanceLogCommandHandler(AppDbContext context, ICurrentCompanyAccessor companyAccessor)
    {
        _context = context;
        _companyAccessor = companyAccessor;
    }

    public async Task<AttendanceLogResponseDto> Handle(UpdateAttendanceLogCommand request, CancellationToken ct)
    {
        var entity = await _context.Set<AttendanceLog>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(AttendanceLog), request.Id);

        var checkInChanged = entity.CheckIn != request.Dto.CheckIn;
        entity.CheckIn = request.Dto.CheckIn;
        entity.CheckOut = request.Dto.CheckOut;
        entity.Status = request.Dto.Status;
        entity.Remarks = request.Dto.Remarks;
        entity.IsManuallyEdited = true;
        entity.UpdatedAt = DateTime.UtcNow;

        var roster = await AttendanceRosterLookup.FindAsync(_context, entity.EmployeeId, entity.AttendanceDate, ct);
        var timeZone = await AttendanceRosterLookup.CompanyTimeZoneAsync(_context, _companyAccessor, ct);
        AttendanceShiftEvaluator.Apply(entity, roster, timeZone, applyLateRule: checkInChanged);

        _context.Set<AttendanceLog>().Update(entity);
        await _context.SaveChangesAsync(ct);

        var employee = await _context.Set<Employee>().FindAsync(new object[] { entity.EmployeeId }, ct);
        return AttendanceMappingHelper.MapAttendanceToDto(entity, employee, roster?.Shift);
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

    internal static ShiftResponseDto MapShiftToDto(Shift e, WorkSchedule? ws) => new()
    {
        Id = e.Id, Name = e.Name, Code = e.Code,
        WorkScheduleId = e.WorkScheduleId, WorkScheduleName = ws?.Name ?? string.Empty,
        StartTime = e.StartTime, EndTime = e.EndTime,
        IsNightShift = e.IsNightShift, IsActive = e.IsActive, CreatedAt = e.CreatedAt
    };

    internal static EmployeeRosterResponseDto MapRosterToDto(EmployeeRoster e, Employee? employee, Shift? shift) => new()
    {
        Id = e.Id, EmployeeId = e.EmployeeId,
        EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
        ShiftId = e.ShiftId, ShiftName = shift?.Name ?? string.Empty,
        ShiftStartTime = shift?.StartTime ?? default, ShiftEndTime = shift?.EndTime ?? default,
        RosterDate = e.RosterDate, IsOff = e.IsOff, Note = e.Note, CreatedAt = e.CreatedAt
    };

    internal static AttendanceLogResponseDto MapAttendanceToDto(AttendanceLog e, Employee? employee, Shift? shift) => new()
    {
        Id = e.Id, EmployeeId = e.EmployeeId,
        EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : string.Empty,
        AttendanceDate = e.AttendanceDate, CheckIn = e.CheckIn, CheckOut = e.CheckOut,
        WorkHours = e.WorkHours, OvertimeHours = e.OvertimeHours, ShiftName = shift?.Name,
        Source = e.Source, Status = e.Status, Remarks = e.Remarks, CreatedAt = e.CreatedAt
    };
}

file static class AttendanceRosterLookup
{
    internal static Task<EmployeeRoster?> FindAsync(AppDbContext context, Guid employeeId, DateTime date, CancellationToken ct) =>
        context.Set<EmployeeRoster>()
            .Include(x => x.Shift).ThenInclude(x => x.WorkSchedule)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.EmployeeId == employeeId && x.RosterDate == date.Date, ct);

    /// <summary>The active company's time zone; falls back to the first company when there's no company claim.</summary>
    internal static async Task<TimeZoneInfo> CompanyTimeZoneAsync(AppDbContext context, ICurrentCompanyAccessor accessor, CancellationToken ct)
    {
        var companies = context.Set<Company>().Where(x => !x.IsDeleted);
        var timeZoneId = accessor.CompanyId is { } companyId
            ? await companies.Where(x => x.Id == companyId).Select(x => x.TimeZone).FirstOrDefaultAsync(ct)
            : await companies.OrderBy(x => x.CreatedAt).Select(x => x.TimeZone).FirstOrDefaultAsync(ct);
        return AttendanceShiftEvaluator.ResolveTimeZone(timeZoneId);
    }
}
