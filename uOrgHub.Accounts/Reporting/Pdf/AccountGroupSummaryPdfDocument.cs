using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class AccountGroupSummaryPdfDocument
{
    public static byte[] Build(List<AccountGroupSummaryRowDto> rows)
    {
        var columns = new List<PdfColumn<AccountGroupSummaryRowDto>>
        {
            new("Code", r => r.GroupCode, 1),
            new("Group", r => r.GroupName, 2.5f),
            new("Type", r => r.GroupType.ToString(), 1),
            new("Accounts", r => r.AccountCount.ToString(), 1, true),
            new("Debit", r => PdfFormat.Amount(r.TotalDebit), 1.3f, true),
            new("Credit", r => PdfFormat.Amount(r.TotalCredit), 1.3f, true),
            new("Balance", r => PdfFormat.Amount(r.Balance), 1.3f, true),
        };

        var totals = new Dictionary<int, string>
        {
            [1] = "Totals",
            [4] = PdfFormat.Amount(rows.Sum(r => r.TotalDebit)),
            [5] = PdfFormat.Amount(rows.Sum(r => r.TotalCredit)),
            [6] = PdfFormat.Amount(rows.Sum(r => r.Balance)),
        };

        return ReportPdfPage.Generate("Account Group Summary", null, null, null,
            content => PdfTable.Render(content, rows, columns, totals));
    }
}
