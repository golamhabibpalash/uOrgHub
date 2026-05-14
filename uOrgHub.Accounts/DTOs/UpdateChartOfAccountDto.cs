using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs;

public class UpdateChartOfAccountDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public Guid AccountGroupId { get; set; }
    public Guid? ParentAccountId { get; set; }
    public AccountGroupType AccountType { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public bool AllowDirectEntry { get; set; } = true;
}