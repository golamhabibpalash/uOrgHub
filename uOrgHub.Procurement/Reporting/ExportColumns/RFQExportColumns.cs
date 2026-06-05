using uOrgHub.Procurement.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Procurement.Reporting.ExportColumns;

public static class RFQExportColumns
{
    public static List<ExportColumn<RFQResponseDto>> Get() =>
    [
        new("rfqNumber", "RFQ Number", x => x.RFQNumber),
        new("rfqDate", "RFQ Date", x => x.RFQDate),
        new("closingDate", "Closing Date", x => x.ClosingDate),
        new("prNumber", "PR Number", x => x.PRNumber),
        new("title", "Title", x => x.Title),
        new("description", "Description", x => x.Description),
        new("status", "Status", x => x.StatusName),
        new("notes", "Notes", x => x.Notes),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
