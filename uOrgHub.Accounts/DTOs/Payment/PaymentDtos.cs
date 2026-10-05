using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.Payment;

public class CreatePaymentDto
{
    public string PaymentNumber { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ChequeNumber { get; set; }
    public string? Notes { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? VendorId { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid FiscalYearId { get; set; }

    /// <summary>
    /// Also generate a posted Debit (money out) or Credit (money in) voucher for this payment.
    /// Requires a bank account and a party with a payable/receivable account, since the voucher
    /// documents the payment's journal entry.
    /// </summary>
    public bool CreateVoucher { get; set; }

    /// <summary>
    /// MR No. for money received. Leave empty to take the next number of the MR series; type one
    /// only when a paper receipt with that number was already handed over.
    /// </summary>
    public string? MoneyReceiptNumber { get; set; }

    public List<CreatePaymentAllocationDto> Allocations { get; set; } = new();
}

public class CreatePaymentAllocationDto
{
    public Guid? InvoiceId { get; set; }
    public Guid? BillId { get; set; }
    public decimal AllocatedAmount { get; set; }
}

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ChequeNumber { get; set; }
    public string? Notes { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? VendorId { get; set; }
    public string? VendorName { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid FiscalYearId { get; set; }
    public Guid? JournalEntryId { get; set; }
    public Guid? VoucherId { get; set; }
    public string? VoucherNumber { get; set; }
    public string? MoneyReceiptNumber { get; set; }
    public List<PaymentAllocationResponseDto> Allocations { get; set; } = new();
}

public class PaymentAllocationResponseDto
{
    public Guid Id { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? BillId { get; set; }
    public decimal AllocatedAmount { get; set; }
}

/// <summary>
/// Everything a printed money receipt needs, resolved on the server so the "paid before / balance
/// after" figures reflect the order payments were actually received in, not today's totals.
/// </summary>
public class PaymentReceiptDto
{
    public Guid PaymentId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public PaymentType PaymentType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ChequeNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankName { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string? VoucherNumber { get; set; }

    public string PartyName { get; set; } = string.Empty;
    public string? PartyAddress { get; set; }
    public string? PartyPhone { get; set; }

    public decimal AllocatedAmount { get; set; }
    /// <summary>Received but not applied to any bill — an advance held on account.</summary>
    public decimal UnallocatedAmount { get; set; }

    public List<PaymentReceiptLineDto> Lines { get; set; } = new();
    public string PreparedBy { get; set; } = string.Empty;
}

public class PaymentReceiptLineDto
{
    public Guid DocumentId { get; set; }
    /// <summary>"Invoice" or "Bill" (a vendor refund settles a bill).</summary>
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; }
    /// <summary>The project / cost center the document is charged to, when it has one.</summary>
    public string? ProjectName { get; set; }
    /// <summary>Source reference shown beside the number, e.g. the RA bill an invoice was raised from.</summary>
    public string? SourceReference { get; set; }
    public decimal DocumentTotal { get; set; }
    public decimal PaidBefore { get; set; }
    public decimal ThisReceipt { get; set; }
    public decimal BalanceAfter { get; set; }
}

/// <summary>Where the MR No. series continues.</summary>
public class MoneyReceiptSeriesDto
{
    public int NextNumber { get; set; }
}

