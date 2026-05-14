using FluentValidation;

namespace uOrgHub.Auth.DTOs;

public record CreateUserDto(
    string Username,
    string Email,
    string? PhoneNumber,
    string FirstName,
    string LastName,
    string Password,
    Guid? EmployeeId,
    List<Guid> RoleIds,
    bool IsTwoFactorEnabled = false
);

public record UpdateUserDto(
    string? Email,
    string? PhoneNumber,
    string? FirstName,
    string? LastName,
    bool? IsTwoFactorEnabled,
    string? TwoFactorMethod
);

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string? PhoneNumber,
    string FirstName,
    string LastName,
    string FullName,
    Guid? EmployeeId,
    bool IsActive,
    bool IsTwoFactorEnabled,
    string TwoFactorMethod,
    int FailedLoginAttempts,
    bool IsLockedOut,
    DateTime? LastLoginAt,
    string? ProfilePicture,
    List<string> Roles,
    List<string> Claims,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AssignRoleDto(List<Guid> RoleIds);
public record AssignUserClaimDto(Guid ClaimId, bool IsGranted = true);

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Must contain at least one number.");
        RuleFor(x => x.RoleIds).NotEmpty().WithMessage("At least one role is required.");
    }
}
