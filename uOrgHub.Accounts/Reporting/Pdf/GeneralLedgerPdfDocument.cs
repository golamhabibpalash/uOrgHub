using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class GeneralLedgerPdfDocument
{
    public static byte[] Build(List<GeneralLedgerRowDto> rows, DateTime? dateFrom, DateTime? dateTo)
    {
        var columns = new List<PdfColumn<GeneralLedgerRowDto>>
        {
            new("Code", r => r.AccountCode, 1),
            new("Account", r => r.AccountName, 2.5f),
            new("Group", r => r.AccountGroupName, 1.5f),
            new("Opening", r => PdfFormat.Amount(r.OpeningBalance), 1.3f, true),
            new("Debit", r => PdfFormat.AmountOrBlank(r.Debit), 1.3f, true),
            new("Credit", r => PdfFormat.AmountOrBlank(r.Credit), 1.3f, true),
            new("Closing", r => PdfFormat.Amount(r.ClosingBalance), 1.3f, true),
        };

        var totals = new Dictionary<int, string>
        {
            [2] = "Totals",
            [4] = PdfFormat.Amount(rows.Sum(r => r.Debit)),
            [5] = PdfFormat.Amount(rows.Sum(r => r.Credit)),
        };

        return ReportPdfPage.Generate("General Ledger", null, dateFrom, dateTo,
            content => PdfTable.Render(content, rows, columns, totals));
    }
}
