using OrgHub.Domain.Entities.Identity;

namespace OrgHub.Application.Features.Identity.Interfaces
{
    public interface IJWTServices
    {
        Task<string> GenerateAccessToken(ApplicationUser user);
        RefreshToken GenerateRefreshToken();
    }
}
