using uOrgHub.HR.DTOs.Attendance;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class ShiftExportColumns
{
    public static List<ExportColumn<ShiftResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("code", "Code", x => x.Code),
        new("workScheduleName", "Work Schedule", x => x.WorkScheduleName),
        new("startTime", "Start Time", x => x.StartTime),
        new("endTime", "End Time", x => x.EndTime),
        new("isNightShift", "Night Shift", x => x.IsNightShift ? "Yes" : "No"),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
