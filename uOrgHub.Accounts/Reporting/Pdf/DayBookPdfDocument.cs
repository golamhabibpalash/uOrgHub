using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class DayBookPdfDocument
{
    public static byte[] Build(List<DayBookRowDto> rows, decimal totalDebit, decimal totalCredit, DateTime? dateFrom, DateTime? dateTo)
    {
        var columns = new List<PdfColumn<DayBookRowDto>>
        {
            new("Date", r => PdfFormat.Date(r.EntryDate), 1),
            new("Entry #", r => r.EntryNumber, 1.2f),
            new("Type", r => r.Type, 0.7f),
            new("Reference", r => r.ReferenceNumber ?? "", 1.2f),
            new("Description", r => r.Description, 2.3f),
            new("Debit", r => PdfFormat.Amount(r.DebitTotal), 1.2f, true),
            new("Credit", r => PdfFormat.Amount(r.CreditTotal), 1.2f, true),
            new("Status", r => r.Status, 1),
        };

        var totals = new Dictionary<int, string>
        {
            [3] = "Totals",
            [4] = "",
            [5] = PdfFormat.Amount(totalDebit),
            [6] = PdfFormat.Amount(totalCredit),
        };

        return ReportPdfPage.Generate("Day Book", null, dateFrom, dateTo,
            content => PdfTable.Render(content, rows, columns, totals));
    }
}
