using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs.Attendance;

public class CreateWorkScheduleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalHours { get; set; } = 8;
    public bool IsFlexible { get; set; } = false;
    public int GracePeriodMinutes { get; set; } = 10;
    public int WorkingDaysPerWeek { get; set; } = 5;
    public bool IsActive { get; set; } = true;
}

public class WorkScheduleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalHours { get; set; }
    public bool IsFlexible { get; set; }
    public int GracePeriodMinutes { get; set; }
    public int WorkingDaysPerWeek { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateShiftDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid WorkScheduleId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsNightShift { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public class ShiftResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid WorkScheduleId { get; set; }
    public string WorkScheduleName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsNightShift { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeRosterDto
{
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public DateTime RosterDate { get; set; }
    public bool IsOff { get; set; } = false;
    public string? Note { get; set; }
}

public class UpdateEmployeeRosterDto
{
    public Guid ShiftId { get; set; }
    public bool IsOff { get; set; }
    public string? Note { get; set; }
}

public class EmployeeRosterResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public DateTime RosterDate { get; set; }
    public bool IsOff { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAttendanceLogDto
{
    public Guid EmployeeId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public AttendanceSource Source { get; set; } = AttendanceSource.Manual;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Remarks { get; set; }
}

public class UpdateAttendanceLogDto
{
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}

public class AttendanceLogResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public decimal WorkHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public AttendanceSource Source { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}
