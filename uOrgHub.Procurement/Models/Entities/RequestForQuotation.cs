using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_request_for_quotations")]
public class RequestForQuotation : BaseEntity
{
    [Required] [MaxLength(30)] public string RFQNumber { get; set; } = string.Empty;
    public DateTime RFQDate { get; set; } = DateTime.UtcNow;
    public DateTime ClosingDate { get; set; }
    public Guid? PRId { get; set; }
    public PurchaseRequisition? PurchaseRequisition { get; set; }
    [Required] [MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
    public RFQStatus Status { get; set; } = RFQStatus.Draft;
    [MaxLength(1000)] public string? Notes { get; set; }

    public ICollection<RFQItem> Items { get; set; } = new List<RFQItem>();
    public ICollection<VendorQuotation> Quotations { get; set; } = new List<VendorQuotation>();
}
