using OrgHub.Application.Features.Identity.DTOs;

namespace OrgHub.Application.Features.Identity.Interfaces;

public interface IAuthService 
{
    Task<AuthResponseDto> LoginAsync(string email, string password);
    Task<AuthResponseDto> RefreshTokenAsync(string token);
    Task<bool> RegisterUserAsync(RegisterUserDto dto);
    Task<bool> UpdateUserAsync(UpdateUserDto dto);
}
