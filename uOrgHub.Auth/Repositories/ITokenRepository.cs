using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Repositories;

public interface ITokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddRefreshTokenAsync(RefreshToken token);
    Task RevokeAsync(RefreshToken token, string reason);
    Task RevokeAllUserTokensAsync(Guid userId, string reason);
    Task AddOtpAsync(TwoFactorOTP otp);
    Task<TwoFactorOTP?> GetValidOtpAsync(Guid userId, string code, string otpType);
    Task MarkOtpUsedAsync(TwoFactorOTP otp);
    Task AddSessionAsync(UserSession session);
    Task<List<UserSession>> GetActiveSessionsAsync(Guid userId);
    Task<UserSession?> GetSessionByTokenAsync(string sessionToken);
    Task DeactivateSessionAsync(UserSession session, string reason);
    Task DeactivateAllUserSessionsAsync(Guid userId, string reason);
}
