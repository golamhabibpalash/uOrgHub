using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.Identity.Interfaces
{
    public interface IJWTServices
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken();
    }
}
