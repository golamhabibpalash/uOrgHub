using uOrgHub.Accounts.DTOs.Budget;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class BudgetExportColumns
{
    public static List<ExportColumn<BudgetResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("description", "Description", x => x.Description),
        new("status", "Status", x => x.Status.ToString()),
        new("totalAmount", "Total Amount", x => x.TotalAmount),
    ];
}
