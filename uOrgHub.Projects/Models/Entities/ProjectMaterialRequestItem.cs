using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_material_request_items")]
public class ProjectMaterialRequestItem : BaseEntity
{
    public Guid RequestId { get; set; }
    public ProjectMaterialRequest Request { get; set; } = null!;

    public Guid ItemVariantId { get; set; }

    public Guid? BOQItemId { get; set; }
    public BOQItem? BOQItem { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal RequestedQuantity { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal ApprovedQuantity { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal IssuedQuantity { get; set; }

    [MaxLength(50)]  public string? UnitOfMeasure { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
