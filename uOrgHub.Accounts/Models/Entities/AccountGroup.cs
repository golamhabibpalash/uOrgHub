using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Entities;

[Table("acc_accountgroups")]
public class AccountGroup : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? CustomCode { get; set; }

    public AccountGroupType Type { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid? ParentAccountGroupId { get; set; }

    [ForeignKey(nameof(ParentAccountGroupId))]
    public AccountGroup? ParentAccountGroup { get; set; }

    public ICollection<AccountGroup> ChildAccountGroups { get; set; } = new List<AccountGroup>();

    public ICollection<ChartOfAccount> ChartOfAccounts { get; set; } = new List<ChartOfAccount>();
}