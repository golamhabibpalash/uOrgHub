using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_purchase_requisition_items")]
public class PurchaseRequisitionItem : BaseEntity
{
    public Guid PRId { get; set; }
    public PurchaseRequisition PurchaseRequisition { get; set; } = null!;
    public Guid ItemVariantId { get; set; }
    public Guid WarehouseId { get; set; }
    [Column(TypeName = "decimal(18,3)")] public decimal RequestedQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal EstimatedUnitCost { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal EstimatedTotalCost { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
