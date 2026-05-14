using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_rfq_items")]
public class RFQItem : BaseEntity
{
    public Guid RFQId { get; set; }
    public RequestForQuotation RequestForQuotation { get; set; } = null!;
    public Guid ItemVariantId { get; set; }
    [Column(TypeName = "decimal(18,3)")] public decimal RequestedQuantity { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}
