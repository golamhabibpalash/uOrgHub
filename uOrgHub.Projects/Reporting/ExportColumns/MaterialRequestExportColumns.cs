using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class MaterialRequestExportColumns
{
    public static List<ExportColumn<MaterialRequestResponseDto>> Get() =>
    [
        new("requestNumber", "Request Number", x => x.RequestNumber),
        new("requestDate", "Request Date", x => x.RequestDate),
        new("requiredDate", "Required Date", x => x.RequiredDate),
        new("status", "Status", x => x.Status.ToString()),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
