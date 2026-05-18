using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_resource_allocations")]
public class SiteResourceAllocation : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    public ResourceType ResourceType { get; set; }

    [Required][MaxLength(300)] public string Description { get; set; } = string.Empty;

    public Guid? EmployeeId { get; set; }
    [MaxLength(100)] public string? EquipmentCode { get; set; }
    [MaxLength(50)]  public string? UnitOfMeasure { get; set; }

    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal PlannedQuantity { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal ActualQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal UnitCost { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PlannedCost { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ActualCost { get; set; }

    public ResourceAllocationStatus Status { get; set; } = ResourceAllocationStatus.Planned;

    [MaxLength(500)] public string? Notes { get; set; }
}
