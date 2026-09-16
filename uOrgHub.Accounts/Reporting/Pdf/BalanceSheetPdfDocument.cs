using QuestPDF.Fluent;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class BalanceSheetPdfDocument
{
    public static byte[] Build(BalanceSheetDto report, DateTime? dateFrom, DateTime? dateTo)
    {
        return ReportPdfPage.Generate("Balance Sheet", null, dateFrom, dateTo, content =>
        {
            content.Column(column =>
            {
                column.Spacing(6);
                PdfLineTree.Render(
                    column.Item(), report.Lines,
                    l => l.Label, l => l.Amount, l => l.IsBold,
                    l => l.Children);

                column.Item().PaddingTop(8).BorderTop(1).PaddingTop(4).Column(summary =>
                {
                    summary.Spacing(2);
                    summary.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Total Assets").Bold();
                        row.RelativeItem(1).AlignRight().Text(PdfFormat.Amount(report.TotalAssets)).Bold();
                    });
                    summary.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Total Liabilities").Bold();
                        row.RelativeItem(1).AlignRight().Text(PdfFormat.Amount(report.TotalLiabilities)).Bold();
                    });
                    summary.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Total Equity").Bold();
                        row.RelativeItem(1).AlignRight().Text(PdfFormat.Amount(report.TotalEquity)).Bold();
                    });
                });
            });
        });
    }
}
