using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_bills")]
public class Bill : BaseEntity
{
    [Required][MaxLength(30)]  public string BillNumber { get; set; } = string.Empty;
    [MaxLength(50)]            public string? VendorBillNumber { get; set; }
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;

    public Guid FiscalYearId { get; set; }
    public FiscalYear FiscalYear { get; set; } = null!;

    public DateTime BillDate { get; set; }
    public DateTime DueDate { get; set; }
    public BillStatus Status { get; set; } = BillStatus.Draft;

    [Column(TypeName = "decimal(18,2)")] public decimal SubTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal DiscountAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PaidAmount { get; set; } = 0;
    [MaxLength(1000)] public string? Notes { get; set; }

    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }

    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    public ICollection<BillLine> Lines { get; set; } = new List<BillLine>();
    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}
