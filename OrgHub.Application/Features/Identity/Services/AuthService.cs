using Microsoft.AspNetCore.Identity;
using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Interfaces;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace OrgHub.Application.Auth.Services;
public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJWTServices _jwtService;

    public AuthService(UserManager<User> userManager, IJWTServices jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> LoginAsync(string email, string password)
    {
        var user = await _userManager.Users.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Email == email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);

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

        var accessToken = _jwtService.GenerateAccessToken(user);
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
        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.UserName,
            RefreshTokens = new List<RefreshToken>()
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

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

}
