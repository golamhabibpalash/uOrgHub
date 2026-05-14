using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_grn_items")]
public class GRNItem : BaseEntity
{
    public Guid GRNId { get; set; }
    public GoodsReceivedNote GoodsReceivedNote { get; set; } = null!;
    public Guid POItemId { get; set; }
    public PurchaseOrderItem POItem { get; set; } = null!;
    public Guid ItemVariantId { get; set; }
    [Column(TypeName = "decimal(18,3)")] public decimal OrderedQuantity { get; set; }
    [Column(TypeName = "decimal(18,3)")] public decimal ReceivedQuantity { get; set; }
    [Column(TypeName = "decimal(18,3)")] public decimal RejectedQuantity { get; set; } = 0;
    [Column(TypeName = "decimal(18,3)")] public decimal AcceptedQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal UnitCost { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
