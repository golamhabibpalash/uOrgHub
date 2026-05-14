using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.AR;

public class CreateCustomerDto
{
    public string CustomerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TIN { get; set; }
    public string? BIN { get; set; }
    public decimal CreditLimit { get; set; } = 0;
    public int PaymentTermsDays { get; set; } = 30;
    public Guid ReceivableAccountId { get; set; }
}

public class UpdateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TIN { get; set; }
    public string? BIN { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerResponseDto
{
    public Guid Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TIN { get; set; }
    public string? BIN { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsActive { get; set; }
    public Guid ReceivableAccountId { get; set; }
}

public class CreateInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid FiscalYearId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public Guid? CostCenterId { get; set; }
    public List<CreateInvoiceLineDto> Lines { get; set; } = new();
}

public class CreateInvoiceLineDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public int LineOrder { get; set; }
    public Guid? TaxRateId { get; set; }
    public Guid RevenueAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
}

public class UpdateInvoiceDto
{
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public Guid? CostCenterId { get; set; }
    public List<CreateInvoiceLineDto> Lines { get; set; } = new();
}

public class InvoiceResponseDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid FiscalYearId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public string? Notes { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? JournalEntryId { get; set; }
    public List<InvoiceLineResponseDto> Lines { get; set; } = new();
}

public class InvoiceLineResponseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int LineOrder { get; set; }
    public Guid? TaxRateId { get; set; }
    public Guid RevenueAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
}
