using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

/// <summary>Renders either a single account's ledger (flat table) or the all-accounts ledger
/// (one section per account, each with its own opening/closing balance and totals).</summary>
public static class AccountLedgerPdfDocument
{
    private static readonly List<PdfColumn<AccountLedgerRowDto>> RowColumns =
    [
        new("Date", r => PdfFormat.Date(r.EntryDate), 1),
        new("Entry #", r => r.EntryNumber, 1.2f),
        new("Reference", r => r.ReferenceNumber ?? "", 1.2f),
        new("Narration", r => r.Narration, 2.8f),
        new("Debit", r => PdfFormat.AmountOrBlank(r.Debit), 1.2f, true),
        new("Credit", r => PdfFormat.AmountOrBlank(r.Credit), 1.2f, true),
        new("Balance", r => PdfFormat.Amount(r.RunningBalance), 1.3f, true),
    ];

    public static byte[] BuildSingle(string accountCode, string accountName, List<AccountLedgerRowDto> rows, DateTime? dateFrom, DateTime? dateTo)
    {
        var totals = new Dictionary<int, string>
        {
            [2] = "Totals",
            [4] = PdfFormat.Amount(rows.Sum(r => r.Debit)),
            [5] = PdfFormat.Amount(rows.Sum(r => r.Credit)),
        };

        return ReportPdfPage.Generate("Account Ledger", $"{accountCode} - {accountName}", dateFrom, dateTo,
            content => PdfTable.Render(content, rows, RowColumns, totals));
    }

    public static byte[] BuildAll(List<AccountLedgerGroupDto> groups, DateTime? dateFrom, DateTime? dateTo)
    {
        return ReportPdfPage.Generate("Account Ledger - All Accounts", null, dateFrom, dateTo, content =>
        {
            content.Column(column =>
            {
                column.Spacing(14);
                foreach (var group in groups)
                {
                    column.Item().Column(section =>
                    {
                        section.Item().SectionHeaderCell()
                            .Text($"{group.AccountCode} - {group.AccountName} ({group.AccountGroupName})");

                        section.Item().PaddingBottom(2).Row(row =>
                        {
                            row.RelativeItem().Text($"Opening Balance: {PdfFormat.Amount(group.OpeningBalance)}").FontSize(8);
                            row.RelativeItem().AlignRight().Text($"Closing Balance: {PdfFormat.Amount(group.ClosingBalance)}").FontSize(8);
                        });

                        var totals = new Dictionary<int, string>
                        {
                            [2] = "Totals",
                            [4] = PdfFormat.Amount(group.TotalDebit),
                            [5] = PdfFormat.Amount(group.TotalCredit),
                        };
                        PdfTable.Render(section.Item(), group.Rows, RowColumns, totals);
                    });
                }
            });
        });
    }
}
