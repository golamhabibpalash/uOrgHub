using uOrgHub.HR.DTOs.Performance;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class GoalExportColumns
{
    public static List<ExportColumn<GoalResponseDto>> Get() =>
    [
        new("employeeName", "Employee", x => x.EmployeeName),
        new("reviewCycleName", "Review Cycle", x => x.ReviewCycleName),
        new("title", "Title", x => x.Title),
        new("description", "Description", x => x.Description),
        new("targetValue", "Target Value", x => x.TargetValue),
        new("achievedValue", "Achieved Value", x => x.AchievedValue),
        new("weight", "Weight", x => x.Weight),
        new("status", "Status", x => x.Status.ToString()),
        new("dueDate", "Due Date", x => x.DueDate),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
