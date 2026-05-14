using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateProjectBudgetDto
{
    public Guid ProjectId { get; set; }
    public BudgetType BudgetType { get; set; }
    public Guid? FiscalYearId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal? RevisedAmount { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProjectBudgetDto
{
    public decimal AllocatedAmount { get; set; }
    public decimal? RevisedAmount { get; set; }
    public string? Notes { get; set; }
}

public class ProjectBudgetResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public BudgetType BudgetType { get; set; }
    public Guid? FiscalYearId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal? RevisedAmount { get; set; }
    public decimal RemainingAmount => (RevisedAmount ?? AllocatedAmount) - SpentAmount;
    public bool IsOverBudget => SpentAmount > (RevisedAmount ?? AllocatedAmount);
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
