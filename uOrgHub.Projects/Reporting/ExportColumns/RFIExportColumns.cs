using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class RFIExportColumns
{
    public static List<ExportColumn<RFIResponseDto>> Get() =>
    [
        new("rfiNumber", "RFI Number", x => x.RFINumber),
        new("subject", "Subject", x => x.Subject),
        new("description", "Description", x => x.Description),
        new("raisedDate", "Raised Date", x => x.RaisedDate),
        new("responseDueDate", "Response Due", x => x.ResponseDueDate),
        new("responseDate", "Response Date", x => x.ResponseDate),
        new("status", "Status", x => x.Status.ToString()),
        new("isUrgent", "Urgent", x => x.IsUrgent ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
