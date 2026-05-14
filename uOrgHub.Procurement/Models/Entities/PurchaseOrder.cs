using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_purchase_orders")]
public class PurchaseOrder : BaseEntity
{
    [Required] [MaxLength(30)] public string PONumber { get; set; } = string.Empty;
    public DateTime PODate { get; set; } = DateTime.UtcNow;
    public DateTime ExpectedDeliveryDate { get; set; }
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public Guid? QuotationId { get; set; }
    public VendorQuotation? Quotation { get; set; }
    public Guid? PRId { get; set; }
    public PurchaseRequisition? PurchaseRequisition { get; set; }
    public POStatus Status { get; set; } = POStatus.Draft;
    [Column(TypeName = "decimal(18,2)")] public decimal SubTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal DiscountAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
    [MaxLength(200)] public string? PaymentTerms { get; set; }
    [MaxLength(500)] public string? DeliveryAddress { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<GoodsReceivedNote> GoodsReceivedNotes { get; set; } = new List<GoodsReceivedNote>();
}
