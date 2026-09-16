using QuestPDF.Fluent;
using uOrgHub.Projects.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Projects.Reporting.Pdf;

public static class ProjectStatementPdfDocument
{
    private static readonly List<PdfColumn<ProjectStatementAccountDto>> AccountColumns =
    [
        new("Code", r => r.AccountCode, 1),
        new("Account", r => r.AccountName, 2.5f),
        new("Type", r => r.AccountType.ToString(), 1.2f),
        new("Debit", r => PdfFormat.AmountOrBlank(r.Debit), 1.2f, true),
        new("Credit", r => PdfFormat.AmountOrBlank(r.Credit), 1.2f, true),
        new("Net", r => PdfFormat.Amount(r.Net), 1.2f, true),
    ];

    private static readonly List<PdfColumn<ProjectStatementRowDto>> RowColumns =
    [
        new("Date", r => PdfFormat.Date(r.EntryDate), 1),
        new("Entry #", r => r.EntryNumber, 1.1f),
        new("Account", r => r.AccountName, 1.8f),
        new("Cost Center", r => r.CostCenterName, 1.2f),
        new("Narration", r => r.Narration ?? "", 2f),
        new("Debit", r => PdfFormat.AmountOrBlank(r.Debit), 1.1f, true),
        new("Credit", r => PdfFormat.AmountOrBlank(r.Credit), 1.1f, true),
        new("Running Net", r => PdfFormat.Amount(r.RunningNet), 1.2f, true),
    ];

    private static readonly List<PdfColumn<ConsolidatedProjectRowDto>> ConsolidatedColumns =
    [
        new("Code", r => r.ProjectCode, 1),
        new("Project", r => r.ProjectName, 2f),
        new("Contract Value", r => PdfFormat.Amount(r.ContractValue), 1.3f, true),
        new("Opening", r => PdfFormat.Amount(r.OpeningSpend), 1.2f, true),
        new("Expense", r => PdfFormat.Amount(r.PeriodExpense), 1.2f, true),
        new("Income", r => PdfFormat.Amount(r.PeriodIncome), 1.2f, true),
        new("Closing", r => PdfFormat.Amount(r.ClosingSpend), 1.2f, true),
        new("Net Cash", r => PdfFormat.Amount(r.NetCashPosition), 1.2f, true),
    ];

    public static byte[] BuildSingle(ProjectStatementDto report)
    {
        return ReportPdfPage.Generate("Project Statement", $"{report.ProjectCode} - {report.ProjectName}",
            report.DateFrom, report.DateTo, content =>
        {
            content.Column(column =>
            {
                column.Spacing(14);
                column.Item().Column(summary => RenderSummary(summary,
                    report.ContractValue, report.OpeningSpend, report.PeriodExpense,
                    report.PeriodIncome, report.ClosingSpend, report.Receipts,
                    report.Payments, report.NetCashPosition));

                column.Item().Column(section =>
                {
                    section.Item().SectionHeaderCell().Text("By Account");
                    PdfTable.Render(section.Item(), report.ByAccount, AccountColumns);
                });

                column.Item().Column(section =>
                {
                    section.Item().SectionHeaderCell().Text("Transactions");
                    PdfTable.Render(section.Item(), report.Rows, RowColumns);
                });
            });
        });
    }

    public static byte[] BuildConsolidated(ConsolidatedProjectStatementDto report)
    {
        return ReportPdfPage.Generate("Project Statement - All Projects", null, report.DateFrom, report.DateTo, content =>
        {
            content.Column(column =>
            {
                column.Spacing(14);
                column.Item().Column(summary => RenderSummary(summary,
                    report.ContractValue, report.OpeningSpend, report.PeriodExpense,
                    report.PeriodIncome, report.ClosingSpend, report.Receipts,
                    report.Payments, report.NetCashPosition));

                var totals = new Dictionary<int, string>
                {
                    [1] = "Totals",
                    [2] = PdfFormat.Amount(report.ContractValue),
                    [3] = PdfFormat.Amount(report.OpeningSpend),
                    [4] = PdfFormat.Amount(report.PeriodExpense),
                    [5] = PdfFormat.Amount(report.PeriodIncome),
                    [6] = PdfFormat.Amount(report.ClosingSpend),
                    [7] = PdfFormat.Amount(report.NetCashPosition),
                };
                PdfTable.Render(column.Item(), report.Projects, ConsolidatedColumns, totals);
            });
        });
    }

    private static void RenderSummary(
        ColumnDescriptor summary, decimal contractValue, decimal openingSpend, decimal periodExpense,
        decimal periodIncome, decimal closingSpend, decimal receipts, decimal payments, decimal netCashPosition)
    {
        summary.Spacing(2);
        summary.Item().Row(row =>
        {
            row.RelativeItem().Text($"Contract Value: {PdfFormat.Amount(contractValue)}").FontSize(9);
            row.RelativeItem().Text($"Opening Spend: {PdfFormat.Amount(openingSpend)}").FontSize(9);
            row.RelativeItem().Text($"Period Expense: {PdfFormat.Amount(periodExpense)}").FontSize(9);
            row.RelativeItem().Text($"Period Income: {PdfFormat.Amount(periodIncome)}").FontSize(9);
        });
        summary.Item().Row(row =>
        {
            row.RelativeItem().Text($"Closing Spend: {PdfFormat.Amount(closingSpend)}").FontSize(9).Bold();
            row.RelativeItem().Text($"Receipts: {PdfFormat.Amount(receipts)}").FontSize(9);
            row.RelativeItem().Text($"Payments: {PdfFormat.Amount(payments)}").FontSize(9);
            row.RelativeItem().Text($"Net Cash Position: {PdfFormat.Amount(netCashPosition)}").FontSize(9).Bold();
        });
    }
}
