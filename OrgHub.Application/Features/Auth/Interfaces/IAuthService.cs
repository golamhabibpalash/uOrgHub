using OrgHub.Application.Features.Auth.DTOs;

namespace OrgHub.Application.Features.Auth.Interfaces;

public interface IAuthService 
{
    Task<AuthResponseDto> LoginAsync(string email, string password);
    Task<AuthResponseDto> RefreshTokenAsync(string token);
    Task<bool> RegisterUserAsync(RegisterUserDto dto);
    Task<bool> UpdateUserAsync(UpdateUserDto dto);
}
