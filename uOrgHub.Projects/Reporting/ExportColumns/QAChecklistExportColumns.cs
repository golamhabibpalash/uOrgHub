using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class QAChecklistExportColumns
{
    public static List<ExportColumn<QAChecklistResponseDto>> Get() =>
    [
        new("checklistNumber", "Checklist Number", x => x.ChecklistNumber),
        new("title", "Title", x => x.Title),
        new("inspectionType", "Inspection Type", x => x.InspectionType.ToString()),
        new("status", "Status", x => x.Status.ToString()),
        new("scheduledDate", "Scheduled Date", x => x.ScheduledDate),
        new("inspectedDate", "Inspected Date", x => x.InspectedDate),
        new("overallResult", "Overall Result", x => x.OverallResult != null ? x.OverallResult.ToString() : string.Empty),
        new("remarks", "Remarks", x => x.Remarks),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
