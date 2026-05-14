using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_goods_received_notes")]
public class GoodsReceivedNote : BaseEntity
{
    [Required] [MaxLength(30)] public string GRNNumber { get; set; } = string.Empty;
    public DateTime GRNDate { get; set; } = DateTime.UtcNow;
    public Guid POId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public Guid ReceivedById { get; set; }
    public GRNStatus Status { get; set; } = GRNStatus.Draft;
    [MaxLength(1000)] public string? Notes { get; set; }
    [MaxLength(100)] public string? InvoiceNumber { get; set; }
    public DateTime? InvoiceDate { get; set; }

    public ICollection<GRNItem> Items { get; set; } = new List<GRNItem>();
}
