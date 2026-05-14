using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_cost_centers")]
public class CostCenter : BaseEntity
{
    [Required][MaxLength(20)]  public string Code { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)]           public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? ParentCostCenterId { get; set; }
    public CostCenter? ParentCostCenter { get; set; }

    public Guid? DepartmentId { get; set; }

    public ICollection<CostCenter> Children { get; set; } = new List<CostCenter>();
    public ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    public ICollection<BillLine> BillLines { get; set; } = new List<BillLine>();
    public ICollection<BudgetLine> BudgetLines { get; set; } = new List<BudgetLine>();
}
