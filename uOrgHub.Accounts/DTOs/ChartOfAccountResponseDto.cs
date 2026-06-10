using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs;

public class ChartOfAccountResponseDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid AccountGroupId { get; set; }
    public string? AccountGroupName { get; set; }
    public Guid? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public AccountGroupType AccountType { get; set; }
    public bool IsActive { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? Description { get; set; }
    public bool AllowDirectEntry { get; set; }
    public string? CustomCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}