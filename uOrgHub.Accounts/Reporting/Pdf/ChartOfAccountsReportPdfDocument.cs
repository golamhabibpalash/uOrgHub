using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Shared.Export.Pdf;

namespace uOrgHub.Accounts.Reporting.Pdf;

public static class ChartOfAccountsReportPdfDocument
{
    public static byte[] Build(List<ChartOfAccountsReportRowDto> rows)
    {
        var columns = new List<PdfColumn<ChartOfAccountsReportRowDto>>
        {
            new("Code", r => r.AccountCode, 1),
            new("Custom Code", r => r.CustomCode ?? "", 1),
            new("Account", r => r.AccountName, 2.5f),
            new("Group", r => r.AccountGroupName, 1.5f),
            new("Type", r => r.AccountType.ToString(), 1),
            new("Balance", r => PdfFormat.Amount(r.CurrentBalance), 1.3f, true),
            new("Active", r => PdfFormat.Bool(r.IsActive), 0.8f),
        };

        return ReportPdfPage.Generate("Chart of Accounts Report", null, null, null,
            content => PdfTable.Render(content, rows, columns));
    }
}
