using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs.AP;

public class CreateVendorDto
{
    public string VendorCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TIN { get; set; }
    public string? BIN { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public Guid PayableAccountId { get; set; }
}

public class UpdateVendorDto
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TIN { get; set; }
    public string? BIN { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsActive { get; set; }
}

public class VendorResponseDto
{
    public Guid Id { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TIN { get; set; }
    public string? BIN { get; set; }
    public int PaymentTermsDays { get; set; }
    public bool IsActive { get; set; }
    public Guid PayableAccountId { get; set; }
}

public class CreateBillDto
{
    public string BillNumber { get; set; } = string.Empty;
    public string? VendorBillNumber { get; set; }
    public Guid VendorId { get; set; }
    public Guid FiscalYearId { get; set; }
    public DateTime BillDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public Guid? CostCenterId { get; set; }
    public List<CreateBillLineDto> Lines { get; set; } = new();
}

public class CreateBillLineDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public int LineOrder { get; set; }
    public Guid? TaxRateId { get; set; }
    public Guid ExpenseAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
}

public class UpdateBillDto
{
    public string? VendorBillNumber { get; set; }
    public DateTime BillDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public Guid? CostCenterId { get; set; }
    public List<CreateBillLineDto> Lines { get; set; } = new();
}

public class BillResponseDto
{
    public Guid Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;
    public string? VendorBillNumber { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public Guid FiscalYearId { get; set; }
    public DateTime BillDate { get; set; }
    public DateTime DueDate { get; set; }
    public BillStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public string? Notes { get; set; }
    public Guid? CostCenterId { get; set; }
    public Guid? JournalEntryId { get; set; }
    public List<BillLineResponseDto> Lines { get; set; } = new();
}

public class BillLineResponseDto
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
    public Guid ExpenseAccountId { get; set; }
    public Guid? CostCenterId { get; set; }
}
