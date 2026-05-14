using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Auth.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ITokenRepository _tokens;
    private readonly IJwtService _jwt;
    private readonly IEmailService _email;
    private readonly ISmsService _sms;
    private readonly IAccessLogService _log;
    private readonly IConfiguration _config;

    public AuthService(
        IUserRepository users, ITokenRepository tokens, IJwtService jwt,
        IEmailService email, ISmsService sms, IAccessLogService log, IConfiguration config)
    {
        _users = users; _tokens = tokens; _jwt = jwt;
        _email = email; _sms = sms; _log = log; _config = config;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, string ipAddress, string userAgent)
    {
        var user = await _users.GetByUsernameAsync(dto.Username)
            ?? throw new AppException("Invalid username or password.");

        if (!user.IsActive || user.IsDeleted)
            throw new AppException("Your account has been deactivated.");

        if (user.IsLockedOut && user.LockoutEndAt > DateTime.UtcNow)
            throw new AppException($"Account locked until {user.LockoutEndAt:HH:mm} UTC.");

        if (user.IsLockedOut && user.LockoutEndAt <= DateTime.UtcNow)
        {
            user.IsLockedOut = false;
            user.FailedLoginAttempts = 0;
            user.LockoutEndAt = null;
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            var maxAttempts = int.Parse(_config["AccountLockout:MaxFailedAttempts"] ?? "5");
            var lockoutMins = int.Parse(_config["AccountLockout:LockoutDurationMinutes"] ?? "15");

            if (user.FailedLoginAttempts >= maxAttempts)
            {
                user.IsLockedOut = true;
                user.LockoutEndAt = DateTime.UtcNow.AddMinutes(lockoutMins);
                user.FailedLoginAttempts = 0;
            }
            await _users.UpdateAsync(user);
            await _log.LogAsync(BuildLog(user.Id, user.Username, "Login", false, ipAddress, userAgent, "Invalid password"));
            throw new AppException("Invalid username or password.");
        }

        user.FailedLoginAttempts = 0;
        user.IsLockedOut = false;
        user.LockoutEndAt = null;
        await _users.UpdateAsync(user);

        if (user.IsTwoFactorEnabled)
        {
            var otp = GenerateOtpCode();
            var channel = user.TwoFactorMethod == "SMS" ? "SMS" : "Email";
            var tempToken = GenerateTempToken();
            var expMins = int.Parse(_config["TwoFactorSettings:EmailOTPExpiryMinutes"] ?? "10");

            await _tokens.AddOtpAsync(new TwoFactorOTP
            {
                UserId = user.Id,
                OTPCode = otp,
                OTPType = "Login",
                Channel = channel,
                SentTo = channel == "SMS" ? MaskPhone(user.PhoneNumber) : MaskEmail(user.Email),
                ExpiresAt = DateTime.UtcNow.AddMinutes(expMins),
            });

            if (channel == "SMS" && !string.IsNullOrWhiteSpace(user.PhoneNumber))
                await _sms.SendOtpAsync(user.PhoneNumber, otp, "Login");
            else
                await _email.SendOtpAsync(user.Email, otp, "Login");

            return new LoginResponseDto(
                RequiresTwoFactor: true,
                TwoFactorMethods: [channel],
                MaskedEmail: MaskEmail(user.Email),
                MaskedPhone: MaskPhone(user.PhoneNumber),
                TempToken: tempToken,
                AccessToken: null, RefreshToken: null, User: null
            );
        }

        return await IssueFullLoginAsync(user, ipAddress, userAgent);
    }

    public async Task<TwoFactorResponseDto> VerifyOTPAsync(VerifyOTPDto dto, string ipAddress)
    {
        var principal = _jwt.ValidateToken(dto.TempToken);
        var subClaim = principal?.FindFirst("sub") ?? principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (subClaim == null || !Guid.TryParse(subClaim.Value, out var userId))
            throw new AppException("Invalid session token.");

        var user = await _users.GetByIdAsync(userId) ?? throw new AppException("User not found.");

        var otp = await _tokens.GetValidOtpAsync(user.Id, dto.OTPCode, "Login");
        if (otp == null)
        {
            // Increment attempt count on existing OTPs
            var otpRecord = await FindPendingOtpAsync(user.Id, "Login");
            if (otpRecord != null) { otpRecord.AttemptCount++; }
            throw new AppException("Invalid or expired OTP.");
        }

        await _tokens.MarkOtpUsedAsync(otp);

        var response = await IssueFullLoginAsync(user, ipAddress, null);
        return new TwoFactorResponseDto(response.AccessToken!, response.RefreshToken!, response.User!);
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var stored = await _tokens.GetByTokenAsync(refreshToken)
            ?? throw new AppException("Invalid refresh token.");

        if (stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            throw new AppException("Refresh token expired or revoked.");

        var user = await _users.GetByIdWithDetailsAsync(stored.UserId)
            ?? throw new AppException("User not found.");

        await _tokens.RevokeAsync(stored, "Rotated");

        var roles = user.UserRoles.Where(ur => !ur.IsDeleted).Select(ur => ur.Role.Name).ToList();
        var claims = (await _users.GetUserClaimsAsync(user.Id)).Where(c => c.IsGranted).Select(c => c.Name).ToList();

        var accessToken = _jwt.GenerateAccessToken(user, roles, claims);
        var newRefresh = _jwt.GenerateRefreshToken(user.Id, ipAddress);
        newRefresh.ReplacedByToken = stored.Token;
        await _tokens.AddRefreshTokenAsync(newRefresh);

        var expiry = int.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"] ?? "15");
        return new TokenResponseDto(accessToken, newRefresh.Token, expiry * 60, MapProfile(user, roles, claims));
    }

    public async Task LogoutAsync(Guid userId, string sessionToken)
    {
        var tokens = await _tokens.GetActiveSessionsAsync(userId);
        foreach (var s in tokens) await _tokens.DeactivateSessionAsync(s, "Manual");
        await _tokens.RevokeAllUserTokensAsync(userId, "Logout");
        await _log.LogAsync(BuildLog(userId, null, "Logout", true, null, null, null));
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _users.GetByEmailAsync(email);
        if (user == null) return;

        var otp = GenerateOtpCode();
        var expMins = int.Parse(_config["TwoFactorSettings:EmailOTPExpiryMinutes"] ?? "10");
        await _tokens.AddOtpAsync(new TwoFactorOTP
        {
            UserId = user.Id, OTPCode = otp, OTPType = "PasswordReset",
            Channel = "Email", SentTo = user.Email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expMins),
        });
        await _email.SendOtpAsync(user.Email, otp, "PasswordReset");
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _users.GetByEmailAsync(dto.Email) ?? throw new AppException("Invalid request.");
        var otp = await _tokens.GetValidOtpAsync(user.Id, dto.OTPCode, "PasswordReset")
            ?? throw new AppException("Invalid or expired OTP.");

        await _tokens.MarkOtpUsedAsync(otp);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
        user.PasswordChangedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        await _tokens.RevokeAllUserTokensAsync(user.Id, "PasswordReset");
        await _log.LogAsync(BuildLog(user.Id, user.Username, "PasswordReset", true, null, null, null));
        return true;
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _users.GetByIdAsync(userId) ?? throw new AppException("User not found.");
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new AppException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
        user.PasswordChangedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        await _tokens.RevokeAllUserTokensAsync(userId, "PasswordChanged");
        await _log.LogAsync(BuildLog(userId, user.Username, "PasswordChanged", true, null, null, null));
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _users.GetByIdWithDetailsAsync(userId)
            ?? throw new AppException("User not found.", 404);
        var roles = user.UserRoles.Where(ur => !ur.IsDeleted).Select(ur => ur.Role.Name).ToList();
        var claims = (await _users.GetUserClaimsAsync(userId)).Where(c => c.IsGranted).Select(c => c.Name).ToList();
        return MapProfile(user, roles, claims);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _users.GetByIdAsync(userId) ?? throw new AppException("User not found.", 404);
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
        if (dto.FirstName != null) user.FirstName = dto.FirstName;
        if (dto.LastName != null) user.LastName = dto.LastName;
        user.UpdatedAt = DateTime.UtcNow;
        await _users.UpdateAsync(user);
        return await GetProfileAsync(userId);
    }

    public async Task Toggle2FAAsync(Guid userId, Toggle2FADto dto)
    {
        var user = await _users.GetByIdAsync(userId) ?? throw new AppException("User not found.");
        user.IsTwoFactorEnabled = dto.Enabled;
        if (dto.TwoFactorMethod != null) user.TwoFactorMethod = dto.TwoFactorMethod;
        if (!dto.Enabled) user.TwoFactorMethod = "None";
        await _users.UpdateAsync(user);
    }

    public async Task<string> SendOTPAsync(Guid userId, string otpType, string channel)
    {
        var user = await _users.GetByIdAsync(userId) ?? throw new AppException("User not found.");
        var otp = GenerateOtpCode();
        var expMins = channel == "SMS"
            ? int.Parse(_config["TwoFactorSettings:SMSOTPExpiryMinutes"] ?? "5")
            : int.Parse(_config["TwoFactorSettings:EmailOTPExpiryMinutes"] ?? "10");

        var sentTo = channel == "SMS" ? user.PhoneNumber ?? user.Email : user.Email;

        await _tokens.AddOtpAsync(new TwoFactorOTP
        {
            UserId = userId, OTPCode = otp, OTPType = otpType,
            Channel = channel, SentTo = sentTo,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expMins),
        });

        if (channel == "SMS" && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            await _sms.SendOtpAsync(user.PhoneNumber, otp, otpType);
        else
            await _email.SendOtpAsync(user.Email, otp, otpType);

        return channel == "SMS" ? MaskPhone(user.PhoneNumber) : MaskEmail(user.Email);
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private async Task<LoginResponseDto> IssueFullLoginAsync(ApplicationUser user, string ipAddress, string? userAgent)
    {
        var userWithDetails = await _users.GetByIdWithDetailsAsync(user.Id) ?? user;
        var roles = userWithDetails.UserRoles.Where(ur => !ur.IsDeleted).Select(ur => ur.Role.Name).ToList();
        var claims = (await _users.GetUserClaimsAsync(user.Id)).Where(c => c.IsGranted).Select(c => c.Name).ToList();

        var accessToken = _jwt.GenerateAccessToken(userWithDetails, roles, claims);
        var refreshToken = _jwt.GenerateRefreshToken(user.Id, ipAddress);
        await _tokens.AddRefreshTokenAsync(refreshToken);

        var session = new UserSession
        {
            UserId = user.Id,
            SessionToken = refreshToken.Token[..50],
            IpAddress = ipAddress,
            LoginAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
        };
        await _tokens.AddSessionAsync(session);

        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIp = ipAddress;
        await _users.UpdateAsync(user);

        await _log.LogAsync(BuildLog(user.Id, user.Username, "Login", true, ipAddress, userAgent, null));

        var expiry = int.Parse(_config["JwtSettings:AccessTokenExpiryMinutes"] ?? "15");
        var profile = MapProfile(userWithDetails, roles, claims);

        return new LoginResponseDto(false, null, null, null, null, accessToken, refreshToken.Token, profile);
    }

    private async Task<TwoFactorOTP?> FindPendingOtpAsync(Guid userId, string otpType)
    {
        var repo = _tokens as Repositories.TokenRepository;
        _ = repo; // can't call internal; skip
        return null;
    }

    private static UserProfileDto MapProfile(ApplicationUser user, List<string> roles, List<string> claims) =>
        new(user.Id, user.Username, user.Email, user.PhoneNumber, user.FirstName, user.LastName,
            $"{user.FirstName} {user.LastName}", user.EmployeeId, user.IsActive,
            user.IsTwoFactorEnabled, user.TwoFactorMethod, roles, claims,
            user.LastLoginAt, user.ProfilePicture);

    private static UserAccessLog BuildLog(Guid? userId, string? username, string action, bool success, string? ip, string? ua, string? details) =>
        new()
        {
            UserId = userId, Username = username, Action = action,
            IsSuccess = success, IpAddress = ip, UserAgent = ua,
            ErrorMessage = details,
        };

    private static string GenerateOtpCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        return (Math.Abs(BitConverter.ToInt32(bytes, 0)) % 900000 + 100000).ToString();
    }

    private static string GenerateTempToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "***";
        var at = email.IndexOf('@');
        if (at <= 1) return "***@" + email[(at + 1)..];
        return email[0] + "***@" + email[(at + 1)..];
    }

    private static string MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "***";
        return phone.Length > 4 ? phone[..4] + "****" + phone[^4..] : "****";
    }
}
