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
    public List<PaymentAllocationResponseDto> Allocations { get; set; } = new();
}

public class PaymentAllocationResponseDto
{
    public Guid Id { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? BillId { get; set; }
    public decimal AllocatedAmount { get; set; }
}
