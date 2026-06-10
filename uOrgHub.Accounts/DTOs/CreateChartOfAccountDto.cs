using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs;

public class CreateChartOfAccountDto
{
    public string AccountName { get; set; } = string.Empty;
    public Guid AccountGroupId { get; set; }
    public Guid? ParentAccountId { get; set; }
    public AccountGroupType AccountType { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal OpeningBalance { get; set; } = 0;
    public string? Description { get; set; }
    public bool AllowDirectEntry { get; set; } = true;
    public string? CustomCode { get; set; }
}