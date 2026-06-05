using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class FiscalYearExportColumns
{
    public static List<ExportColumn<FiscalYearResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("startDate", "Start Date", x => x.StartDate),
        new("endDate", "End Date", x => x.EndDate),
        new("status", "Status", x => x.Status.ToString()),
        new("isCurrent", "Current", x => x.IsCurrent ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
        new("createdBy", "Created By", x => x.CreatedBy),
    ];
}
