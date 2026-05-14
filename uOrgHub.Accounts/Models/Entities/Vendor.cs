using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_vendors")]
public class Vendor : BaseEntity
{
    [Required][MaxLength(20)]  public string VendorCode { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(100)] public string? ContactPerson { get; set; }
    [MaxLength(200)] public string? Email { get; set; }
    [MaxLength(20)]  public string? Phone { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
    [MaxLength(20)]  public string? TIN { get; set; }
    [MaxLength(20)]  public string? BIN { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;

    public Guid PayableAccountId { get; set; }
    public ChartOfAccount PayableAccount { get; set; } = null!;

    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
