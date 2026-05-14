using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_project_budgets")]
public class ProjectBudget : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public BudgetType BudgetType { get; set; }

    public Guid? FiscalYearId { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal AllocatedAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal SpentAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? RevisedAmount { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }
}
