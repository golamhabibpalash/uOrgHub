using QuestPDF.Fluent;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class ReceiptsPaymentsPdfDocument
{
    private static readonly List<PdfColumn<TransferRowDto>> TransferColumns =
    [
        new("Date", r => PdfFormat.Date(r.EntryDate), 1),
        new("Entry #", r => r.EntryNumber, 1.2f),
        new("From", r => r.FromAccount, 1.5f),
        new("To", r => r.ToAccount, 1.5f),
        new("Narration", r => r.Narration, 2.3f),
        new("Amount", r => PdfFormat.Amount(r.Amount), 1.2f, true),
    ];

    private static readonly List<PdfColumn<ReceiptsPaymentsRowDto>> GroupRowColumns =
    [
        new("Code", r => r.AccountCode, 1),
        new("Account", r => r.AccountName, 3),
        new("Amount", r => PdfFormat.Amount(r.Amount), 1.2f, true),
    ];

    private static readonly List<PdfColumn<CashBankBalanceRowDto>> BalanceColumns =
    [
        new("Code", r => r.AccountCode, 1),
        new("Account", r => r.AccountName, 2),
        new("Opening", r => PdfFormat.Amount(r.Opening), 1.2f, true),
        new("Receipts", r => PdfFormat.Amount(r.Receipts), 1.2f, true),
        new("Payments", r => PdfFormat.Amount(r.Payments), 1.2f, true),
        new("Closing", r => PdfFormat.Amount(r.Closing), 1.2f, true),
    ];

    public static byte[] Build(ReceiptsPaymentsReportDto report, DateTime? dateFrom, DateTime? dateTo)
    {
        return ReportPdfPage.Generate("Receipts & Payments Statement", null, dateFrom, dateTo, content =>
        {
            content.Column(column =>
            {
                column.Spacing(14);

                if (report.Transfers.Count > 0)
                {
                    column.Item().Column(section =>
                    {
                        section.Item().SectionHeaderCell().Text("Transfers");
                        var totals = new Dictionary<int, string> { [3] = "Total Transfers", [5] = PdfFormat.Amount(report.TotalTransfers) };
                        PdfTable.Render(section.Item(), report.Transfers, TransferColumns, totals);
                    });
                }

                column.Item().Column(section =>
                {
                    section.Item().SectionHeaderCell().Text("Receipts");
                    RenderGroups(section, report.Receipts);
                    section.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Total Receipts (excl. transfers)").Bold();
                        row.RelativeItem(1).AlignRight().Text(PdfFormat.Amount(report.TotalReceiptsExclTransfers)).Bold();
                    });
                    section.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Total Receipts (incl. transfers)").Bold();
                        row.RelativeItem(1).AlignRight().Text(PdfFormat.Amount(report.TotalReceiptsInclTransfers)).Bold();
                    });
                });

                column.Item().Column(section =>
                {
                    section.Item().SectionHeaderCell().Text("Payments");
                    RenderGroups(section, report.Payments);
                    section.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text("Total Payments").Bold();
                        row.RelativeItem(1).AlignRight().Text(PdfFormat.Amount(report.TotalPayments)).Bold();
                    });
                });

                column.Item().Column(section =>
                {
                    section.Item().SectionHeaderCell().Text("Cash & Bank Balances");
                    var totals = new Dictionary<int, string>
                    {
                        [1] = "Totals",
                        [2] = PdfFormat.Amount(report.TotalOpening),
                        [3] = PdfFormat.Amount(report.TotalPeriodReceipts),
                        [4] = PdfFormat.Amount(report.TotalPeriodPayments),
                        [5] = PdfFormat.Amount(report.TotalClosing),
                    };
                    PdfTable.Render(section.Item(), report.Balances, BalanceColumns, totals);
                });
            });
        });
    }

    private static void RenderGroups(ColumnDescriptor parent, List<ReceiptsPaymentsGroupDto> groups)
    {
        foreach (var group in groups)
        {
            parent.Item().PaddingTop(4).Text(group.CostCenterName).FontSize(9).Bold();
            var totals = new Dictionary<int, string> { [1] = "Subtotal", [2] = PdfFormat.Amount(group.Subtotal) };
            PdfTable.Render(parent.Item(), group.Rows, GroupRowColumns, totals);
        }
    }
}
