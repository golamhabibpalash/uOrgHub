using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_vouchers")]
public class Voucher : BaseEntity
{
    [Required]
    [MaxLength(30)]
    public string VoucherNumber { get; set; } = string.Empty;

    public VoucherType VoucherType { get; set; }

    public DateTime VoucherDate { get; set; }

    public Guid? FiscalYearId { get; set; }
    public FiscalYear? FiscalYear { get; set; }

    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(200)]
    public string? Section { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public Guid DebitAccountId { get; set; }
    public ChartOfAccount DebitAccount { get; set; } = null!;

    public Guid CreditAccountId { get; set; }
    public ChartOfAccount CreditAccount { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; } = 0;

    public VoucherStatus Status { get; set; } = VoucherStatus.Draft;

    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }

    [MaxLength(200)]
    public string? PreparedBy { get; set; }

    [MaxLength(200)]
    public string? ReceivedBy { get; set; }

    [MaxLength(100)]
    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAt { get; set; }

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [MaxLength(100)]
    public string? RejectedBy { get; set; }

    public DateTime? RejectedAt { get; set; }

    [MaxLength(500)]
    public string? RejectReason { get; set; }

    [MaxLength(100)]
    public string? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }
}