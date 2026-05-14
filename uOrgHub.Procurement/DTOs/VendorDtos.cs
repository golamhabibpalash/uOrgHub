using uOrgHub.Procurement.Models.Enums;

namespace uOrgHub.Procurement.DTOs;

public record CreateVendorDto(
    string CompanyName,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? TradeLicense,
    string? TIN,
    string? BIN,
    VendorType VendorType,
    decimal CreditLimit,
    int PaymentTermDays,
    string? Notes
);

public record UpdateVendorDto(
    string CompanyName,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? TradeLicense,
    string? TIN,
    string? BIN,
    VendorType VendorType,
    VendorStatus Status,
    decimal CreditLimit,
    int PaymentTermDays,
    string? Notes
);

public class VendorResponseDto
{
    public Guid Id { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TradeLicense { get; set; }
    public string? TIN { get; set; }
    public string? BIN { get; set; }
    public VendorType VendorType { get; set; }
    public string VendorTypeName => VendorType.ToString();
    public VendorStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public decimal CreditLimit { get; set; }
    public int PaymentTermDays { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
