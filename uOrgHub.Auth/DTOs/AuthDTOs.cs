using System.Text.Json.Serialization;
using FluentValidation;

namespace uOrgHub.Auth.DTOs;

public record LoginRequestDto(
    string Username,
    string Password,
    bool RememberMe = false
);

public record LoginResponseDto(
    bool RequiresTwoFactor,
    List<string>? TwoFactorMethods,
    string? MaskedEmail,
    string? MaskedPhone,
    string? TempToken,
    string? AccessToken,
    string? RefreshToken,
    UserProfileDto? User
);

public record VerifyOTPDto(
    string TempToken,
    string OTPCode,
    string Channel
);

public record TwoFactorResponseDto(
    string AccessToken,
    string RefreshToken,
    UserProfileDto User
);

public record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    UserProfileDto? User
);

public record UserProfileDto(
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
    List<string> Roles,
    List<string> Claims,
    DateTime? LastLoginAt,
    string? ProfilePicture
);

public record ResetPasswordDto(
    string Email,
    string OTPCode,
    string NewPassword,
    string ConfirmPassword
);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);

public record ForgotPasswordRequestDto(string Email);

public record UpdateProfileDto(
    string? Email,
    string? PhoneNumber,
    string? FirstName,
    string? LastName,
    bool? IsTwoFactorEnabled,
    string? TwoFactorMethod
);

public record SendOTPDto(
    string OTPType,
    string Channel
);

public record Toggle2FADto(
    bool Enabled,
    string? TwoFactorMethod
);

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class VerifyOTPDtoValidator : AbstractValidator<VerifyOTPDto>
{
    public VerifyOTPDtoValidator()
    {
        RuleFor(x => x.TempToken).NotEmpty();
        RuleFor(x => x.OTPCode).NotEmpty().Length(6);
        RuleFor(x => x.Channel).NotEmpty().Must(c => c is "Email" or "SMS");
    }
}

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.OTPCode).NotEmpty().Length(6);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Must contain at least one number.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Must contain at least one number.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword);
    }
}
