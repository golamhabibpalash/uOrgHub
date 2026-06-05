using uOrgHub.Accounts.DTOs.Banking;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class BankAccountExportColumns
{
    public static List<ExportColumn<BankAccountResponseDto>> Get() =>
    [
        new("accountNumber", "Account Number", x => x.AccountNumber),
        new("accountName", "Account Name", x => x.AccountName),
        new("bankName", "Bank Name", x => x.BankName),
        new("branchName", "Branch", x => x.BranchName),
        new("routingNumber", "Routing Number", x => x.RoutingNumber),
        new("currency", "Currency", x => x.Currency),
        new("openingBalance", "Opening Balance", x => x.OpeningBalance),
        new("currentBalance", "Current Balance", x => x.CurrentBalance),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
    ];
}
