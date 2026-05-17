using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace uOrgHub.Shared.Entities;

[Table("companies")]
public class Company : BaseEntity
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? TaxId { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(500)]
    public string? TagLine { get; set; }

    [Required, MaxLength(10)]
    public string Currency { get; set; } = "BDT";

    [Required, MaxLength(100)]
    public string TimeZone { get; set; } = "Asia/Dhaka";

    public bool IsActive { get; set; } = true;
}
