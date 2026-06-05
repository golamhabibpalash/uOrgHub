using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class ProjectBudgetExportColumns
{
    public static List<ExportColumn<ProjectBudgetResponseDto>> Get() =>
    [
        new("budgetType", "Budget Type", x => x.BudgetType.ToString()),
        new("allocatedAmount", "Allocated Amount", x => x.AllocatedAmount),
        new("revisedAmount", "Revised Amount", x => x.RevisedAmount),
        new("spentAmount", "Spent Amount", x => x.SpentAmount),
        new("remainingAmount", "Remaining", x => x.RemainingAmount),
        new("isOverBudget", "Over Budget", x => x.IsOverBudget ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
