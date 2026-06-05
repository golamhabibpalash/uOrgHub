using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class WBSExportColumns
{
    public static List<ExportColumn<WBSResponseDto>> Get() =>
    [
        new("wbsCode", "WBS Code", x => x.WBSCode),
        new("title", "Title", x => x.Title),
        new("description", "Description", x => x.Description),
        new("level", "Level", x => x.Level),
        new("plannedStartDate", "Planned Start", x => x.PlannedStartDate),
        new("plannedEndDate", "Planned End", x => x.PlannedEndDate),
        new("actualStartDate", "Actual Start", x => x.ActualStartDate),
        new("actualEndDate", "Actual End", x => x.ActualEndDate),
        new("plannedDuration", "Planned Duration", x => x.PlannedDuration),
        new("status", "Status", x => x.Status.ToString()),
        new("completionPercent", "Completion %", x => x.CompletionPercent),
        new("sequence", "Sequence", x => x.Sequence),
    ];
}
