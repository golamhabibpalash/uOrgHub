using uOrgHub.HR.DTOs.Leave;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class LeaveRequestExportColumns
{
    public static List<ExportColumn<LeaveRequestResponseDto>> Get() =>
    [
        new("employeeName", "Employee", x => x.EmployeeName),
        new("leaveTypeName", "Leave Type", x => x.LeaveTypeName),
        new("startDate", "Start Date", x => x.StartDate),
        new("endDate", "End Date", x => x.EndDate),
        new("totalDays", "Total Days", x => x.TotalDays),
        new("reason", "Reason", x => x.Reason),
        new("status", "Status", x => x.Status.ToString()),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
