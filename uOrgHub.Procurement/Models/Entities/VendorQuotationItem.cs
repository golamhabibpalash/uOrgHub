using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_vendor_quotation_items")]
public class VendorQuotationItem : BaseEntity
{
    public Guid QuotationId { get; set; }
    public VendorQuotation VendorQuotation { get; set; } = null!;
    public Guid RFQItemId { get; set; }
    public RFQItem RFQItem { get; set; } = null!;
    public Guid ItemVariantId { get; set; }
    [Column(TypeName = "decimal(18,3)")] public decimal QuotedQuantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalPrice { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
