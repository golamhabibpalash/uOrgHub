using Microsoft.AspNetCore.Identity;
using OrgHub.Application.Features.Identity.DTOs;
using OrgHub.Application.Features.Identity.Interfaces;
using OrgHub.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using OrgHub.Domain.Entities.Identity;

namespace OrgHub.Application.Auth.Services;
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJWTServices _jwtService; 
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AuthService(UserManager<ApplicationUser> userManager, IJWTServices jwtService, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _roleManager = roleManager;
    }

    public async Task<AuthResponseDto> LoginAsync(string email, string password)
    {
        var user = await _userManager.Users.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Email == email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            throw new UnauthorizedAccessException("Invalid credentials");

        var accessToken = await _jwtService.GenerateAccessToken(user);
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
