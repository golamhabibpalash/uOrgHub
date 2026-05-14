using FluentValidation;

namespace uOrgHub.Auth.DTOs;

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    bool IsActive,
    int UserCount,
    List<ClaimDto> Claims
);

public record RoleListDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    bool IsActive,
    int UserCount
);

public record ClaimDto(
    Guid Id,
    string Name,
    string? Description,
    string? Module,
    string? Category,
    bool IsActive
);

public record CreateRoleDto(string Name, string? Description);
public record UpdateRoleDto(string Name, string? Description);
public record AssignRoleClaimsDto(List<Guid> ClaimIds);

public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
{
    public CreateRoleDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
{
    public UpdateRoleDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
