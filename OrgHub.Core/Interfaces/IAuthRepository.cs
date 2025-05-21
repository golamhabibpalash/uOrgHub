using OrgHub.Domain.Entities.Identity;

namespace OrgHub.Core.Interfaces;

public interface IAuthRepository
{
    Task SaveChangesAsync(ApplicationUser user);
    Task<ApplicationUser?> GetUserByTokenAsync(string token);
}
