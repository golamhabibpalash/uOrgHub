using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class ProjectMilestoneExportColumns
{
    public static List<ExportColumn<ProjectMilestoneResponseDto>> Get() =>
    [
        new("title", "Title", x => x.Title),
        new("description", "Description", x => x.Description),
        new("plannedDate", "Planned Date", x => x.PlannedDate),
        new("actualDate", "Actual Date", x => x.ActualDate),
        new("status", "Status", x => x.Status.ToString()),
        new("isCritical", "Critical", x => x.IsCritical ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
