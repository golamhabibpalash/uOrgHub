using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class JournalEntryExportColumns
{
    public static List<ExportColumn<JournalEntryResponseDto>> Get() =>
    [
        new("entryNumber", "Entry Number", x => x.EntryNumber),
        new("entryDate", "Entry Date", x => x.EntryDate),
        new("referenceNumber", "Reference", x => x.ReferenceNumber),
        new("description", "Description", x => x.Description),
        new("status", "Status", x => x.Status.ToString()),
        new("totalDebit", "Total Debit", x => x.TotalDebit),
        new("totalCredit", "Total Credit", x => x.TotalCredit),
        new("createdBy", "Created By", x => x.CreatedBy),
        new("postedBy", "Posted By", x => x.PostedBy),
        new("postedAt", "Posted At", x => x.PostedAt),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
