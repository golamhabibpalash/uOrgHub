using QuestPDF.Infrastructure;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class TrialBalancePdfDocument
{
    public static byte[] Build(TrialBalanceResponseDto report, DateTime? dateFrom, DateTime? dateTo)
    {
        var columns = new List<PdfColumn<TrialBalanceRowDto>>
        {
            new("Code", r => r.AccountCode, 1),
            new("Account", r => r.AccountName, 2.5f),
            new("Group", r => r.AccountGroupName, 1.5f),
            new("Opening Dr", r => PdfFormat.AmountOrBlank(r.OpeningDebit), 1.3f, true),
            new("Opening Cr", r => PdfFormat.AmountOrBlank(r.OpeningCredit), 1.3f, true),
            new("Debit", r => PdfFormat.AmountOrBlank(r.Debit), 1.3f, true),
            new("Credit", r => PdfFormat.AmountOrBlank(r.Credit), 1.3f, true),
            new("Closing Dr", r => PdfFormat.AmountOrBlank(r.ClosingDebit), 1.3f, true),
            new("Closing Cr", r => PdfFormat.AmountOrBlank(r.ClosingCredit), 1.3f, true),
        };

        var totals = new Dictionary<int, string>
        {
            [2] = "Totals",
            [3] = PdfFormat.Amount(report.TotalOpeningDebit),
            [4] = PdfFormat.Amount(report.TotalOpeningCredit),
            [5] = PdfFormat.Amount(report.TotalDebit),
            [6] = PdfFormat.Amount(report.TotalCredit),
            [7] = PdfFormat.Amount(report.TotalClosingDebit),
            [8] = PdfFormat.Amount(report.TotalClosingCredit),
        };

        return ReportPdfPage.Generate("Trial Balance", null, dateFrom, dateTo,
            content => PdfTable.Render(content, report.Rows, columns, totals));
    }
}
