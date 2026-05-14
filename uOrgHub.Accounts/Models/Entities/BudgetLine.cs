using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_budget_lines")]
public class BudgetLine : BaseEntity
{
    public Guid BudgetId { get; set; }
    public Budget Budget { get; set; } = null!;

    public Guid AccountId { get; set; }
    public ChartOfAccount Account { get; set; } = null!;

    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }

    public int Period { get; set; } = 0;
    [Column(TypeName = "decimal(18,2)")] public decimal PlannedAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ActualAmount { get; set; } = 0;
}
