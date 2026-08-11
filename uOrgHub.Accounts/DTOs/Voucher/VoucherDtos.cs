using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.Voucher;

public class CreateVoucherDto
{
    public VoucherType VoucherType { get; set; }
    public DateTime VoucherDate { get; set; }
    public Guid? FiscalYearId { get; set; }

    /// <summary>
    /// The project being charged. Leave null only for head-office / overhead vouchers, which must
    /// instead name a <see cref="CostCenterId"/> — exactly one of the two is required.
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Set directly for an overhead voucher. For a project voucher this is resolved from the
    /// project's own cost center and anything sent here is ignored.
    /// </summary>
    public Guid? CostCenterId { get; set; }

    public string? Name { get; set; }
    public string? Section { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid DebitAccountId { get; set; }
    public Guid CreditAccountId { get; set; }
    public decimal Amount { get; set; } = 0;
    public string? PreparedBy { get; set; }
    public string? ReceivedBy { get; set; }
}

public class UpdateVoucherDto
{
    public DateTime VoucherDate { get; set; }
    public Guid? FiscalYearId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? Name { get; set; }
    public string? Section { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid DebitAccountId { get; set; }
    public Guid CreditAccountId { get; set; }
    public decimal Amount { get; set; } = 0;
    public string? PreparedBy { get; set; }
    public string? ReceivedBy { get; set; }
}

public class RejectVoucherDto
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// A chart-of-account offered for one side of a voucher, together with why it qualifies.
/// </summary>
public class VoucherAccountOptionDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountGroupType AccountType { get; set; }
    public string AccountGroupName { get; set; } = string.Empty;

    /// <summary>
    /// Section heading for this option in the dropdown, e.g. "Income — revenue earned". Groups
    /// arrive in relevance order so the user reaches the likely accounts first.
    /// </summary>
    public string GroupLabel { get; set; } = string.Empty;

    /// <summary>True when a bank account record is attached, i.e. real cash moves through it.</summary>
    public bool IsBankLinked { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
}

/// <summary>
/// The accounts a given voucher type may use on each side, with the label each side carries.
/// Expressed per side rather than per role so a Contra voucher — which has money on both sides
/// and no party side — needs no special case. Returned as one payload so the form cannot pair a
/// stale list for one side with a fresh list for the other.
/// </summary>
public class VoucherAccountOptionsDto
{
    public VoucherType VoucherType { get; set; }

    public List<VoucherAccountOptionDto> DebitAccounts { get; set; } = new();
    public List<VoucherAccountOptionDto> CreditAccounts { get; set; } = new();

    /// <summary>e.g. "Receive Into", "Party Account", "Transfer To".</summary>
    public string DebitFieldLabel { get; set; } = string.Empty;
    public string CreditFieldLabel { get; set; } = string.Empty;

    /// <summary>
    /// True for a Contra voucher, where money only moves between the organisation's own accounts.
    /// </summary>
    public bool IsOwnAccountTransfer { get; set; }
}

public class VoucherResponseDto
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public VoucherType VoucherType { get; set; }
    public DateTime VoucherDate { get; set; }
    public Guid? FiscalYearId { get; set; }
    public string? FiscalYearName { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public string? CostCenterCode { get; set; }
    public string? Name { get; set; }
    public string? Section { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid DebitAccountId { get; set; }
    public string DebitAccountName { get; set; } = string.Empty;
    public Guid CreditAccountId { get; set; }
    public string CreditAccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 0;
    public VoucherStatus Status { get; set; }
    public Guid? JournalEntryId { get; set; }
    public string? JournalEntryNumber { get; set; }
    public string? PreparedBy { get; set; }
    public string? ReceivedBy { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectReason { get; set; }
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
