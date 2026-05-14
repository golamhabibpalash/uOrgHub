using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_vendor_quotations")]
public class VendorQuotation : BaseEntity
{
    [Required] [MaxLength(30)] public string QuotationNumber { get; set; } = string.Empty;
    public Guid RFQId { get; set; }
    public RequestForQuotation RequestForQuotation { get; set; } = null!;
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public DateTime QuotationDate { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Received;
    [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
    public int DeliveryDays { get; set; }
    [MaxLength(200)] public string? PaymentTerms { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    public ICollection<VendorQuotationItem> Items { get; set; } = new List<VendorQuotationItem>();
}
