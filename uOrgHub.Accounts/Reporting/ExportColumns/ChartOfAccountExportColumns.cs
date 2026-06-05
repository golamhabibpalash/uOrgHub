using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class ChartOfAccountExportColumns
{
    public static List<ExportColumn<ChartOfAccountResponseDto>> Get() =>
    [
        new("accountCode", "Account Code", x => x.AccountCode),
        new("accountName", "Account Name", x => x.AccountName),
        new("accountGroup", "Account Group", x => x.AccountGroupName),
        new("accountType", "Account Type", x => x.AccountType.ToString()),
        new("parentAccount", "Parent Account", x => x.ParentAccountName),
        new("description", "Description", x => x.Description),
        new("openingBalance", "Opening Balance", x => x.OpeningBalance),
        new("currentBalance", "Current Balance", x => x.CurrentBalance),
        new("allowDirectEntry", "Allow Direct Entry", x => x.AllowDirectEntry ? "Yes" : "No"),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
        new("createdBy", "Created By", x => x.CreatedBy),
    ];
}
