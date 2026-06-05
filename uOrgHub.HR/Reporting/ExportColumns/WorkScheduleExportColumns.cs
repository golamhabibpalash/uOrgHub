using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class WorkScheduleExportColumns
{
    public static List<ExportColumn<WorkScheduleResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("description", "Description", x => x.Description),
        new("startTime", "Start Time", x => x.StartTime),
        new("endTime", "End Time", x => x.EndTime),
        new("totalHours", "Total Hours", x => x.TotalHours),
        new("isFlexible", "Flexible", x => x.IsFlexible ? "Yes" : "No"),
        new("gracePeriodMinutes", "Grace Period (min)", x => x.GracePeriodMinutes),
        new("workingDaysPerWeek", "Working Days/Week", x => x.WorkingDaysPerWeek),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
