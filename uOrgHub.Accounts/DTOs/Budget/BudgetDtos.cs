using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.Budget;

public class CreateBudgetDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid FiscalYearId { get; set; }
    public Guid? CostCenterId { get; set; }
    public List<CreateBudgetLineDto> Lines { get; set; } = new();
}

public class CreateBudgetLineDto
{
    public Guid AccountId { get; set; }
    public Guid? CostCenterId { get; set; }
    public int Period { get; set; } = 0;
    public decimal PlannedAmount { get; set; }
}

public class UpdateBudgetDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CostCenterId { get; set; }
}

public class ApproveBudgetDto
{
    public Guid BudgetId { get; set; }
}

public class BudgetResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BudgetStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid FiscalYearId { get; set; }
    public Guid? CostCenterId { get; set; }
    public List<BudgetLineResponseDto> Lines { get; set; } = new();
}

public class BudgetLineResponseDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public int Period { get; set; }
    public decimal PlannedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance => PlannedAmount - ActualAmount;
}
