using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_budgets")]
public class Budget : BaseEntity
{
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)]           public string? Description { get; set; }
    public BudgetStatus Status { get; set; } = BudgetStatus.Draft;
    [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; } = 0;

    public Guid FiscalYearId { get; set; }
    public FiscalYear FiscalYear { get; set; } = null!;

    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }

    public ICollection<BudgetLine> Lines { get; set; } = new List<BudgetLine>();
}
