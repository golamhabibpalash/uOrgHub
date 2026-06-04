using uOrgHub.HR.DTOs;

namespace uOrgHub.API.DTOs;

public class CreateEmployeeWithUserDto
{
    public CreateEmployeeDto Employee { get; set; } = null!;
    public bool CreateUserAccount { get; set; }
    public EmployeeUserAccountDto? UserAccount { get; set; }
}

public class EmployeeUserAccountDto
{
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool AutoGeneratePassword { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> RoleIds { get; set; } = new();
}
