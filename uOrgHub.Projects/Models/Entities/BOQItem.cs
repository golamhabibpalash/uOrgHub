using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_boq_items")]
public class BOQItem : BaseEntity
{
    public Guid BOQId { get; set; }
    public BillOfQuantity BOQ { get; set; } = null!;

    public Guid? ItemVariantId { get; set; }

    [Required][MaxLength(500)] public string ItemDescription { get; set; } = string.Empty;
    [MaxLength(1000)]          public string? Specification { get; set; }
    [MaxLength(50)]            public string? UnitOfMeasure { get; set; }

    [Column(TypeName = "decimal(18,4)")] public decimal EstimatedQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal UnitRate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal EstimatedAmount { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal ActualQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal ActualAmount { get; set; }

    public int Sequence { get; set; }
}
