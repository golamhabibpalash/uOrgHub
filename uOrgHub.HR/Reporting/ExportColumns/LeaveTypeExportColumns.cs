using uOrgHub.HR.DTOs.Leave;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class LeaveTypeExportColumns
{
    public static List<ExportColumn<LeaveTypeResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("code", "Code", x => x.Code),
        new("description", "Description", x => x.Description),
        new("totalDaysPerYear", "Total Days/Year", x => x.TotalDaysPerYear),
        new("maxConsecutiveDays", "Max Consecutive Days", x => x.MaxConsecutiveDays),
        new("isPaidLeave", "Paid Leave", x => x.IsPaidLeave ? "Yes" : "No"),
        new("carryForward", "Carry Forward", x => x.CarryForward ? "Yes" : "No"),
        new("requiresDocument", "Requires Document", x => x.RequiresDocument ? "Yes" : "No"),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
