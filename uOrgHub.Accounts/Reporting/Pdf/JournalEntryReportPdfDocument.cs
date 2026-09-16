using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class JournalEntryReportPdfDocument
{
    public static byte[] Build(List<JournalEntryReportRowDto> rows, DateTime? dateFrom, DateTime? dateTo)
    {
        var columns = new List<PdfColumn<JournalEntryReportRowDto>>
        {
            new("Entry #", r => r.EntryNumber, 1.2f),
            new("Date", r => PdfFormat.Date(r.EntryDate), 1),
            new("Reference", r => r.ReferenceNumber ?? "", 1.2f),
            new("Description", r => r.Description, 2.5f),
            new("Debit", r => PdfFormat.Amount(r.TotalDebit), 1.2f, true),
            new("Credit", r => PdfFormat.Amount(r.TotalCredit), 1.2f, true),
            new("Status", r => r.Status, 1),
            new("Created By", r => r.CreatedBy, 1.2f),
        };

        var totals = new Dictionary<int, string>
        {
            [3] = "Totals",
            [4] = PdfFormat.Amount(rows.Sum(r => r.TotalDebit)),
            [5] = PdfFormat.Amount(rows.Sum(r => r.TotalCredit)),
        };

        return ReportPdfPage.Generate("Journal Entry Report", null, dateFrom, dateTo,
            content => PdfTable.Render(content, rows, columns, totals));
    }
}
