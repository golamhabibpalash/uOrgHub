using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_chartofaccounts")]
public class ChartOfAccount : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string AccountCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;

    public Guid AccountGroupId { get; set; }
    public AccountGroup AccountGroup { get; set; } = null!;

    public Guid? ParentAccountId { get; set; }
    public ChartOfAccount? ParentAccount { get; set; }

    public AccountGroupType AccountType { get; set; }

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpeningBalance { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; } = 0;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool AllowDirectEntry { get; set; } = true;

    public ICollection<ChartOfAccount> SubAccounts { get; set; } = new List<ChartOfAccount>();
    public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}