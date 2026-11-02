using QuestPDF.Fluent;
using uOrgHub.Projects.DTOs.Reports;
using uOrgHub.Projects.Models.Enums;
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

    /// <param name="transactions">
    /// The filters <paramref name="report"/>'s rows were already narrowed by — listed under the
    /// Transactions heading so a filtered printout can't be mistaken for the full ledger.
    /// </param>
    public static byte[] BuildSingle(ProjectStatementDto report, ProjectStatementTransactionQuery? transactions = null)
    {
        var filters = DescribeFilters(transactions, report.ByAccount);

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
                    section.Item().SectionHeaderCell().Text(filters.Count > 0 ? "Transactions (filtered)" : "Transactions");
                    if (filters.Count > 0)
                        section.Item().PaddingVertical(3).Text($"Filtered by: {string.Join("  |  ", filters)}").FontSize(8).Italic();
                    PdfTable.Render(section.Item(), report.Rows, RowColumns);
                    if (report.Rows.Count == 0 && filters.Count > 0)
                        section.Item().PaddingTop(4).Text("No transactions match the selected filters.").FontSize(9).Italic();
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

    /// <summary>
    /// Human-readable list of the active transaction filters, worded like the report screen's
    /// controls. Sort order is left out: it changes the order of rows, not which rows appear.
    /// </summary>
    private static List<string> DescribeFilters(
        ProjectStatementTransactionQuery? q, IReadOnlyList<ProjectStatementAccountDto> accounts)
    {
        var parts = new List<string>();
        if (q is null) return parts;

        if (q.TransactionType == ProjectStatementTransactionFilter.Receipts) parts.Add("Receipts only");
        if (q.TransactionType == ProjectStatementTransactionFilter.Payments) parts.Add("Payments only");
        if (!string.IsNullOrWhiteSpace(q.Search)) parts.Add($"Search \"{q.Search.Trim()}\"");
        if (q.AccountId.HasValue)
        {
            var account = accounts.FirstOrDefault(a => a.AccountId == q.AccountId.Value);
            parts.Add(account is null ? "Account (none in period)" : $"Account {account.AccountCode} - {account.AccountName}");
        }
        if (q.Side == ProjectStatementLineSide.Debit) parts.Add("Debit lines only");
        if (q.Side == ProjectStatementLineSide.Credit) parts.Add("Credit lines only");
        if (!string.IsNullOrWhiteSpace(q.PaidTo)) parts.Add($"Paid to {q.PaidTo.Trim()}");
        if (!string.IsNullOrWhiteSpace(q.ReceivedFrom)) parts.Add($"Received from {q.ReceivedFrom.Trim()}");
        return parts;
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
