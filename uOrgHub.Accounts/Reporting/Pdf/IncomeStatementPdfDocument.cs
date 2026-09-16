using QuestPDF.Fluent;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class IncomeStatementPdfDocument
{
    public static byte[] Build(IncomeStatementDto report, DateTime? dateFrom, DateTime? dateTo)
    {
        return ReportPdfPage.Generate("Income Statement", null, dateFrom, dateTo, content =>
        {
            content.Column(column =>
            {
                column.Spacing(6);
                PdfLineTree.Render(
                    column.Item(), report.Lines,
                    l => l.Label, l => l.Amount, l => l.IsBold,
                    l => l.Children);

                column.Item().PaddingTop(8).BorderTop(1).PaddingTop(4).Row(row =>
                {
                    row.RelativeItem(3).Text("Net Profit / (Loss)").Bold().FontSize(11);
                    row.RelativeItem(1).AlignRight().Text(PdfFormat.Amount(report.NetProfit)).Bold().FontSize(11);
                });
            });
        });
    }
}
