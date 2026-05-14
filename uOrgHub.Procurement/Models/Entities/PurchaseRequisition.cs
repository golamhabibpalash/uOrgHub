using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_purchase_requisitions")]
public class PurchaseRequisition : BaseEntity
{
    [Required] [MaxLength(30)] public string PRNumber { get; set; } = string.Empty;
    public DateTime PRDate { get; set; } = DateTime.UtcNow;
    public DateTime RequiredDate { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid RequestedById { get; set; }
    [MaxLength(500)] public string? Purpose { get; set; }
    public PRStatus Status { get; set; } = PRStatus.Draft;
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    [MaxLength(500)] public string? RejectionReason { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }

    public ICollection<PurchaseRequisitionItem> Items { get; set; } = new List<PurchaseRequisitionItem>();
}
