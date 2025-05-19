using OrgHub.Application.Features.Auth.DTOs;
using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.Auth.Interfaces
{
    public interface IJWTServices
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken();
    }
}
