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
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task Toggle2FAAsync(Guid userId, Toggle2FADto dto);
    Task<string> SendOTPAsync(Guid userId, string otpType, string channel);
}
