using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class ExpenseRequestExportColumns
{
    public static List<ExportColumn<ExpenseRequestResponseDto>> Get() =>
    [
        new("employeeName", "Employee", x => x.EmployeeName),
        new("category", "Category", x => x.Category.ToString()),
        new("amount", "Amount", x => x.Amount),
        new("expenseDate", "Expense Date", x => x.ExpenseDate),
        new("description", "Description", x => x.Description),
        new("status", "Status", x => x.Status.ToString()),
        new("approverName", "Approver", x => x.ApproverName),
        new("approvedAt", "Approved At", x => x.ApprovedAt),
        new("paidAt", "Paid At", x => x.PaidAt),
        new("rejectionReason", "Rejection Reason", x => x.RejectionReason),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
