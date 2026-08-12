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

    /// <summary>
    /// The number written on the physical voucher slip, when one exists. Free text and optional —
    /// it is whatever the paper says, so it is never generated, never validated for uniqueness,
    /// and never used to identify the voucher. <see cref="VoucherNumber"/> remains the system's
    /// own identifier; this only ties the record back to the document in the filing cabinet.
    /// </summary>
    [MaxLength(50)]
    public string? ReferenceNumber { get; set; }

    public Guid? FiscalYearId { get; set; }
    public FiscalYear? FiscalYear { get; set; }

    /// <summary>
    /// The project this voucher is charged to, or null for head-office / overhead vouchers.
    /// Held as a bare key rather than a navigation because uOrgHub.Accounts cannot reference
    /// uOrgHub.Projects — same arrangement as <see cref="CostCenter.ProjectId"/>.
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Where the voucher's cost or receipt is attributed in the GL. For a project voucher this is
    /// the project's cost center; for an overhead voucher it is chosen directly. Copied onto the
    /// generated journal entry lines, which is what makes the amount visible to project reporting.
    /// </summary>
    public Guid? CostCenterId { get; set; }
    public CostCenter? CostCenter { get; set; }

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