using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_customers")]
public class Customer : BaseEntity
{
    [Required][MaxLength(20)]  public string CustomerCode { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(100)] public string? ContactPerson { get; set; }
    [MaxLength(200)] public string? Email { get; set; }
    [MaxLength(20)]  public string? Phone { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
    [MaxLength(20)]  public string? TIN { get; set; }
    [MaxLength(20)]  public string? BIN { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal CreditLimit { get; set; } = 0;
    public int PaymentTermsDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;

    public Guid ReceivableAccountId { get; set; }
    public ChartOfAccount ReceivableAccount { get; set; } = null!;

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
