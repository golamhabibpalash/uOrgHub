using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;

namespace uOrgHub.Auth.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly AppDbContext _db;

    public TokenRepository(AppDbContext db) => _db = db;

    public async Task<RefreshToken?> GetByTokenAsync(string token) =>
        await _db.Set<RefreshToken>().FirstOrDefaultAsync(rt => rt.Token == token);

    public async Task AddRefreshTokenAsync(RefreshToken token)
    {
        _db.Set<RefreshToken>().Add(token);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAsync(RefreshToken token, string reason)
    {
        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedReason = reason;
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, string reason)
    {
        var tokens = await _db.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();
        tokens.ForEach(t =>
        {
            t.IsRevoked = true;
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedReason = reason;
        });
        await _db.SaveChangesAsync();
    }

    public async Task AddOtpAsync(TwoFactorOTP otp)
    {
        var old = await _db.Set<TwoFactorOTP>()
            .Where(o => o.UserId == otp.UserId && o.OTPType == otp.OTPType && !o.IsUsed)
            .ToListAsync();
        old.ForEach(o => { o.IsUsed = true; o.UsedAt = DateTime.UtcNow; });

        _db.Set<TwoFactorOTP>().Add(otp);
        await _db.SaveChangesAsync();
    }

    public async Task<TwoFactorOTP?> GetValidOtpAsync(Guid userId, string code, string otpType) =>
        await _db.Set<TwoFactorOTP>()
            .Where(o => o.UserId == userId && o.OTPCode == code && o.OTPType == otpType
                        && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow && o.AttemptCount < 3)
            .FirstOrDefaultAsync();

    public async Task MarkOtpUsedAsync(TwoFactorOTP otp)
    {
        otp.IsUsed = true;
        otp.UsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task AddSessionAsync(UserSession session)
    {
        _db.Set<UserSession>().Add(session);
        await _db.SaveChangesAsync();
    }

    public async Task<List<UserSession>> GetActiveSessionsAsync(Guid userId) =>
        await _db.Set<UserSession>()
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync();

    public async Task<UserSession?> GetSessionByTokenAsync(string sessionToken) =>
        await _db.Set<UserSession>().FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.IsActive);

    public async Task DeactivateSessionAsync(UserSession session, string reason)
    {
        session.IsActive = false;
        session.LogoutAt = DateTime.UtcNow;
        session.LogoutReason = reason;
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAllUserSessionsAsync(Guid userId, string reason)
    {
        var sessions = await _db.Set<UserSession>().Where(s => s.UserId == userId && s.IsActive).ToListAsync();
        sessions.ForEach(s =>
        {
            s.IsActive = false;
            s.LogoutAt = DateTime.UtcNow;
            s.LogoutReason = reason;
        });
        await _db.SaveChangesAsync();
    }
}
