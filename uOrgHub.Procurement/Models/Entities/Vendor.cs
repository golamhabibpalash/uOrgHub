using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Procurement.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Entities;

[Table("proc_vendors")]
public class Vendor : BaseEntity
{
    [Required] [MaxLength(30)] public string VendorCode { get; set; } = string.Empty;
    [Required] [MaxLength(200)] public string CompanyName { get; set; } = string.Empty;
    [MaxLength(100)] public string? ContactPerson { get; set; }
    [MaxLength(200)] public string? Email { get; set; }
    [MaxLength(20)] public string? Phone { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
    [MaxLength(100)] public string? TradeLicense { get; set; }
    [MaxLength(50)] public string? TIN { get; set; }
    [MaxLength(50)] public string? BIN { get; set; }
    public VendorType VendorType { get; set; } = VendorType.Supplier;
    public VendorStatus Status { get; set; } = VendorStatus.Active;
    [Column(TypeName = "decimal(18,2)")] public decimal CreditLimit { get; set; } = 0;
    public int PaymentTermDays { get; set; } = 30;
    [MaxLength(1000)] public string? Notes { get; set; }
}
