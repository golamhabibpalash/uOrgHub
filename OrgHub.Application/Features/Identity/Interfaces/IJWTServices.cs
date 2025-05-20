using OrgHub.Domain.Entities;

namespace OrgHub.Application.Features.Identity.Interfaces
{
    public interface IJWTServices
    {
        Task<string> GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken();
    }
}
