using uOrgHub.Auth.DTOs;

namespace uOrgHub.Auth.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, string ipAddress, string userAgent);
    Task<TwoFactorResponseDto> VerifyOTPAsync(VerifyOTPDto dto, string ipAddress);
    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task LogoutAsync(Guid userId, string sessionToken);
    Task ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<UserProfileDto> GetProfileAsync(Guid userId, Guid? activeCompanyId = null);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task Toggle2FAAsync(Guid userId, Toggle2FADto dto);
    Task<string> SendOTPAsync(Guid userId, string otpType, string channel);

    /// <summary>Re-issues the caller's token pair scoped to a different sister concern they
    /// belong to (SISTER_CONCERN_PLAN.md). Throws AppException if they have no UserCompany row
    /// for <paramref name="companyId"/>.</summary>
    Task<TokenResponseDto> SwitchCompanyAsync(Guid userId, Guid companyId, string ipAddress);
}
