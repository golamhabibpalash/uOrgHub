using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_project_expenses")]
public class ProjectExpense : BaseEntity
{
    [Required][MaxLength(20)] public string ExpenseNumber { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    public DateTime ExpenseDate { get; set; }
    public ExpenseType ExpenseType { get; set; }

    [Required][MaxLength(500)] public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }

    public Guid? VendorId { get; set; }
    public Guid? POId { get; set; }

    [MaxLength(100)] public string? InvoiceNumber { get; set; }
    public Guid RecordedById { get; set; }

    public Guid? CostCenterId { get; set; }

    public ExpenseStatus Status { get; set; } = ExpenseStatus.Draft;
    public Guid? ApprovedById { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }
}
