using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class BOQExportColumns
{
    public static List<ExportColumn<BOQResponseDto>> Get() =>
    [
        new("boqNumber", "BOQ Number", x => x.BOQNumber),
        new("title", "Title", x => x.Title),
        new("description", "Description", x => x.Description),
        new("status", "Status", x => x.Status.ToString()),
        new("totalEstimatedCost", "Total Estimated Cost", x => x.TotalEstimatedCost),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
