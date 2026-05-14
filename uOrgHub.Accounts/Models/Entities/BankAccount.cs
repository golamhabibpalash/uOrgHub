using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_bank_accounts")]
public class BankAccount : BaseEntity
{
    [Required][MaxLength(50)]  public string AccountNumber { get; set; } = string.Empty;
    [Required][MaxLength(200)] public string AccountName { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string BankName { get; set; } = string.Empty;
    [MaxLength(100)] public string? BranchName { get; set; }
    [MaxLength(30)]  public string? RoutingNumber { get; set; }
    [MaxLength(10)]  public string Currency { get; set; } = "BDT";
    [Column(TypeName = "decimal(18,2)")] public decimal OpeningBalance { get; set; } = 0;
    [Column(TypeName = "decimal(18,2)")] public decimal CurrentBalance { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public Guid ChartOfAccountId { get; set; }
    public ChartOfAccount ChartOfAccount { get; set; } = null!;

    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
}
