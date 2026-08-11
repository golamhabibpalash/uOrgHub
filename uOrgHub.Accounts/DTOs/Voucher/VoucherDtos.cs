using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.Voucher;

public class CreateVoucherDto
{
    public VoucherType VoucherType { get; set; }
    public DateTime VoucherDate { get; set; }
    public Guid? FiscalYearId { get; set; }
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
/// A GL account that money can physically move through — i.e. a chart-of-account
/// backed by an active bank account record. One side of every voucher must be one of these.
/// </summary>
public class VoucherCashAccountDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}

public class VoucherResponseDto
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public VoucherType VoucherType { get; set; }
    public DateTime VoucherDate { get; set; }
    public Guid? FiscalYearId { get; set; }
    public string? FiscalYearName { get; set; }
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