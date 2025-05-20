using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrgHub.Application.Features.Identity.Interfaces;
using OrgHub.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrgHub.Application.Features.Identity.Services;

public class JwtService : IJWTServices
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    public JwtService(IConfiguration configuration, UserManager<User> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }
    public async Task<string> GenerateAccessToken(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var keyString = _configuration["Jwt:Key"];

        if (string.IsNullOrEmpty(keyString))
            throw new InvalidOperationException("JWT Key is not configured in appsettings.json");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpiryMinutes"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken()
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            Expires = DateTime.UtcNow.AddDays(7)
        };
    }
}
