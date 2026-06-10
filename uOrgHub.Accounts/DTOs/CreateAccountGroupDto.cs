using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.DTOs;

public class CreateAccountGroupDto
{
    public string Name { get; set; } = string.Empty;
    public AccountGroupType Type { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ParentAccountGroupId { get; set; }
    public string? CustomCode { get; set; }
}