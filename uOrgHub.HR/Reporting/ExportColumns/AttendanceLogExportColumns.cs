using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class AttendanceLogExportColumns
{
    public static List<ExportColumn<AttendanceLogResponseDto>> Get() =>
    [
        new("employeeName", "Employee", x => x.EmployeeName),
        new("attendanceDate", "Date", x => x.AttendanceDate),
        new("checkIn", "Check In", x => x.CheckIn),
        new("checkOut", "Check Out", x => x.CheckOut),
        new("workHours", "Work Hours", x => x.WorkHours),
        new("overtimeHours", "Overtime Hours", x => x.OvertimeHours),
        new("source", "Source", x => x.Source.ToString()),
        new("status", "Status", x => x.Status.ToString()),
        new("remarks", "Remarks", x => x.Remarks),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
