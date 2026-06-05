using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using uOrgHub.API.Middleware;
using uOrgHub.API.Services;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.DTOs;
using uOrgHub.Auth.Services;
using uOrgHub.Shared.Models;

namespace uOrgHub.API.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly EmployeeProfilePictureService _pictureService;

    public AuthController(IAuthService authService, EmployeeProfilePictureService pictureService)
    {
        _authService = authService;
        _pictureService = pictureService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
    private string GetUserAgent() => Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto, GetIpAddress(), GetUserAgent());
        return Ok(ApiResponse<LoginResponseDto>.Ok(result));
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPDto dto)
    {
        var result = await _authService.VerifyOTPAsync(dto, GetIpAddress());
        return Ok(ApiResponse<TwoFactorResponseDto>.Ok(result));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, GetIpAddress());
        return Ok(ApiResponse<TokenResponseDto>.Ok(result));
    }

    [HttpPost("logout")]
    [Authorize]
    [RequireClaim(Claims.Self.ViewProfile)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _authService.LogoutAsync(GetUserId(), request.SessionToken);
        return Ok(ApiResponse<string>.Ok("Logged out successfully"));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(ApiResponse<string>.Ok("If the email exists, a reset code has been sent."));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var success = await _authService.ResetPasswordAsync(dto);
        if (!success)
            return BadRequest(ApiResponse<string>.Fail("Invalid or expired reset code."));

        return Ok(ApiResponse<string>.Ok("Password reset successfully."));
    }

    [HttpPost("change-password")]
    [Authorize]
    [RequireClaim(Claims.Self.EditProfile)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        await _authService.ChangePasswordAsync(GetUserId(), dto);
        return Ok(ApiResponse<string>.Ok("Password changed successfully."));
    }

    [HttpPost("send-otp")]
    [Authorize]
    [RequireClaim(Claims.Self.EditProfile)]
    public async Task<IActionResult> SendOTP([FromBody] SendOTPDto dto)
    {
        var result = await _authService.SendOTPAsync(GetUserId(), dto.OTPType, dto.Channel);
        return Ok(ApiResponse<string>.Ok($"OTP sent to {result}"));
    }

    [HttpGet("me")]
    [Authorize]
    [RequireClaim(Claims.Self.ViewProfile)]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _authService.GetProfileAsync(GetUserId());
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }

    [HttpPut("me")]
    [Authorize]
    [RequireClaim(Claims.Self.EditProfile)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var profile = await _authService.UpdateProfileAsync(GetUserId(), dto);
        return Ok(ApiResponse<UserProfileDto>.Ok(profile));
    }

    [HttpPut("me/2fa")]
    [Authorize]
    [RequireClaim(Claims.Self.EditProfile)]
    public async Task<IActionResult> Toggle2FA([FromBody] Toggle2FADto dto)
    {
        await _authService.Toggle2FAAsync(GetUserId(), dto);
        return Ok(ApiResponse<string>.Ok(dto.Enabled ? "2FA enabled" : "2FA disabled"));
    }

    [HttpPost("me/profile-picture")]
    [Authorize]
    [RequestSizeLimit(8 * 1024 * 1024)]
    [RequireClaim(Claims.Self.EditProfile)]
    public async Task<IActionResult> UploadMyProfilePicture(IFormFile file, CancellationToken ct)
    {
        var url = await _pictureService.UploadForCurrentUserAsync(GetUserId(), file, ct);
        return Ok(ApiResponse<string>.Ok(url, "Profile picture uploaded successfully."));
    }

    [HttpDelete("me/profile-picture")]
    [Authorize]
    [RequireClaim(Claims.Self.EditProfile)]
    public async Task<IActionResult> DeleteMyProfilePicture(CancellationToken ct)
    {
        await _pictureService.DeleteForCurrentUserAsync(GetUserId(), ct);
        return Ok(ApiResponse<string>.Ok("Removed", "Profile picture removed successfully."));
    }
}

public record TokenRefreshRequest(string RefreshToken);
public record LogoutRequest(string SessionToken);
