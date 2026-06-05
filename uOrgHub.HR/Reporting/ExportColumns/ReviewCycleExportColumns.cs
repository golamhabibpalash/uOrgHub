using uOrgHub.HR.DTOs.Performance;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class ReviewCycleExportColumns
{
    public static List<ExportColumn<ReviewCycleResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("type", "Type", x => x.Type.ToString()),
        new("startDate", "Start Date", x => x.StartDate),
        new("endDate", "End Date", x => x.EndDate),
        new("status", "Status", x => x.Status.ToString()),
        new("description", "Description", x => x.Description),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
