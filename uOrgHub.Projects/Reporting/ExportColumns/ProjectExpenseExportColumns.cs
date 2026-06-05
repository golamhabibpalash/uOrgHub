using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class ProjectExpenseExportColumns
{
    public static List<ExportColumn<ProjectExpenseResponseDto>> Get() =>
    [
        new("expenseNumber", "Expense Number", x => x.ExpenseNumber),
        new("expenseDate", "Expense Date", x => x.ExpenseDate),
        new("expenseType", "Expense Type", x => x.ExpenseType.ToString()),
        new("description", "Description", x => x.Description),
        new("amount", "Amount", x => x.Amount),
        new("invoiceNumber", "Invoice Number", x => x.InvoiceNumber),
        new("status", "Status", x => x.Status.ToString()),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
