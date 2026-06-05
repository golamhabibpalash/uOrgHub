using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class NCRExportColumns
{
    public static List<ExportColumn<NCRResponseDto>> Get() =>
    [
        new("ncrNumber", "NCR Number", x => x.NCRNumber),
        new("title", "Title", x => x.Title),
        new("description", "Description", x => x.Description),
        new("category", "Category", x => x.Category.ToString()),
        new("severity", "Severity", x => x.Severity.ToString()),
        new("raisedDate", "Raised Date", x => x.RaisedDate),
        new("responsibleParty", "Responsible Party", x => x.ResponsibleParty),
        new("correctiveAction", "Corrective Action", x => x.CorrectiveAction),
        new("verifiedDate", "Verified Date", x => x.VerifiedDate),
        new("closedDate", "Closed Date", x => x.ClosedDate),
        new("status", "Status", x => x.Status.ToString()),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
