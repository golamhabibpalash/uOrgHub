using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrgHub.Application.Common.Interfaces;
using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Interfaces;
using OrgHub.Application.Features.Others.Interfaces;
using OrgHub.Domain.Entities.Identity;
using OrgHub.Domain.Enums;

namespace OrgHub.Application.Auth.Services;
public class AuthService(UserManager<ApplicationUser> userManager, IJWTServices jwtService, RoleManager<ApplicationRole> roleManager, ILoggingService loggingService, ICurrentUserService currentUserService) : IAuthService
{
    #region Fields
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJWTServices _jwtService = jwtService;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ILoggingService _loggingService = loggingService;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    #endregion Fields

    public async Task<AuthResponseDto> LoginAsync(string email, string password)
    {
        var user = await _userManager.Users.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Email == email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken = await _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);
        _loggingService.LogActivity("UserLogin", $"User {user.UserName} logged in successfully.", user.Id);
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = refreshToken.Expires,
            UserName = user.UserName!
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string token)
    {
        var user = await _userManager.Users
             .Include(x => x.RefreshTokens)
             .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token && t.Expires > DateTime.UtcNow));

        if (user == null)
            throw new UnauthorizedAccessException("Invalid refresh token");

        var accessToken = await _jwtService.GenerateAccessToken(user);
        var newRefresh = _jwtService.GenerateRefreshToken();
        user.RefreshTokens.Add(newRefresh);
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefresh.Token,
            ExpiresAt = newRefresh.Expires,
            UserName = user.UserName!
        };
    }

    public async Task<bool> RegisterUserAsync(RegisterUserDto dto)
    {
        var user = new ApplicationUser
        {
            FullName = dto.FullName,
            CreatedDate = DateTime.Now,
            //CreatedBy = _currentUserService.UserId ?? throw new InvalidOperationException("Current user ID is null."),
            CreatedBy = _currentUserService.UserId ?? Guid.NewGuid(),
            Email = dto.Email,
            UserName = dto.UserName,
            RefreshTokens = new List<RefreshToken>()
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (result.Succeeded)
        {
            _loggingService.LogActivity(LogActivityAction.Insert.ToString(), $" New user {user.UserName} registered successfully.", user.Id);
        }
        return result.Succeeded;
    }

    public async Task<bool> UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id.ToString());

        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.UserName;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            return false;

        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            return passwordResult.Succeeded;
        }

        return true;
    }

    public async Task<bool> CreateRoleAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
            return true; // Already exists

        var result = await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        return result.Succeeded;
    }
    public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

}
