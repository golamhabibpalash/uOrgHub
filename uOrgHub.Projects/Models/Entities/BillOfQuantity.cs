using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_boqs")]
public class BillOfQuantity : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    [Required][MaxLength(20)]  public string BOQNumber { get; set; } = string.Empty;
    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(1000)]          public string? Description { get; set; }

    public BOQStatus Status { get; set; } = BOQStatus.Draft;

    [Column(TypeName = "decimal(18,2)")] public decimal TotalEstimatedCost { get; set; }

    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<BOQItem> Items { get; set; } = new List<BOQItem>();
}
