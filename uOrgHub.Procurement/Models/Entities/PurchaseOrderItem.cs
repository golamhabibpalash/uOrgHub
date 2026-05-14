using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_purchase_order_items")]
public class PurchaseOrderItem : BaseEntity
{
    public Guid POId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Guid ItemVariantId { get; set; }
    [Column(TypeName = "decimal(18,3)")] public decimal OrderedQuantity { get; set; }
    [Column(TypeName = "decimal(18,3)")] public decimal ReceivedQuantity { get; set; } = 0;
    [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,4)")] public decimal TaxPercent { get; set; } = 0;
    [Column(TypeName = "decimal(18,4)")] public decimal DiscountPercent { get; set; } = 0;
    [Column(TypeName = "decimal(18,2)")] public decimal TotalPrice { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
