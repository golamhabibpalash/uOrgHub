using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_ra_bills")]
public class RABill : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Required][MaxLength(20)]  public string BillNumber { get; set; } = string.Empty;
    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;

    public DateTime BillDate { get; set; }
    public DateTime PeriodFrom { get; set; }
    public DateTime PeriodTo { get; set; }

    public int BillSequence { get; set; }

    public Guid SubmittedById { get; set; }
    public Guid? CertifiedById { get; set; }
    public DateTime? CertifiedDate { get; set; }
    public DateTime? PaidDate { get; set; }

    [Column(TypeName = "decimal(18,2)")] public decimal GrossAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal DeductionAmount { get; set; }
    [Column(TypeName = "decimal(5,2)")]  public decimal RetentionPercent { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal RetentionAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal NetAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PreviousBilledAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal CumulativeBilledAmount { get; set; }

    public RABillStatus Status { get; set; } = RABillStatus.Draft;

    [MaxLength(500)] public string? Notes { get; set; }

    public ICollection<RABillItem> Items { get; set; } = new List<RABillItem>();
}
