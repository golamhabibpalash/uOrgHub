using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

/// <summary>Shared layout for AR/AP Aging (same DTO, differs only by title and party label).</summary>
internal static class AgingPdfDocument
{
    public static byte[] Build(string title, string partyLabel, AgingSummaryDto report, DateTime asOfDate)
    {
        var columns = new List<PdfColumn<AgingRowDto>>
        {
            new(partyLabel, r => r.CustomerOrVendor, 1.8f),
            new("Document #", r => r.DocumentNumber, 1.2f),
            new("Doc. Date", r => PdfFormat.Date(r.DocumentDate), 1),
            new("Due Date", r => PdfFormat.Date(r.DueDate), 1),
            new("Total", r => PdfFormat.Amount(r.TotalAmount), 1.1f, true),
            new("Paid", r => PdfFormat.Amount(r.PaidAmount), 1.1f, true),
            new("Balance Due", r => PdfFormat.Amount(r.BalanceDue), 1.2f, true),
            new("Days", r => r.DaysOverdue.ToString(), 0.7f, true),
            new("Bucket", r => r.AgingBucket, 1.2f),
        };

        var totals = new Dictionary<int, string> { [0] = "Totals", [6] = PdfFormat.Amount(report.TotalOutstanding) };

        return ReportPdfPage.Generate(title, $"As of {PdfFormat.Date(asOfDate)}", null, null, content =>
        {
            content.Column(column =>
            {
                column.Spacing(10);
                column.Item().Row(row =>
                {
                    AddBucket(row, "Current", report.CurrentAmount);
                    AddBucket(row, "1-30 Days", report.Days1To30);
                    AddBucket(row, "31-60 Days", report.Days31To60);
                    AddBucket(row, "61-90 Days", report.Days61To90);
                    AddBucket(row, "90+ Days", report.DaysOver90);
                    AddBucket(row, "Total Outstanding", report.TotalOutstanding, true);
                });
                PdfTable.Render(column.Item(), report.Rows, columns, totals);
            });
        });
    }

    private static void AddBucket(RowDescriptor row, string label, decimal amount, bool emphasize = false)
    {
        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(box =>
        {
            box.Item().Text(label).FontSize(7).FontColor(Colors.Grey.Darken1);
            var value = box.Item().Text(PdfFormat.Amount(amount)).FontSize(emphasize ? 11 : 10);
            if (emphasize) value.Bold();
        });
    }
}
