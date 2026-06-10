using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs;

public class GeneratedCodeDto
{
    public string Code { get; set; } = string.Empty;
    public AccountGroupType Type { get; set; }
    public Guid? ParentAccountGroupId { get; set; }
}
