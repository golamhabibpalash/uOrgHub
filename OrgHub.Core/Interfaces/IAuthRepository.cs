using OrgHub.Domain.Entities.Identity;

namespace OrgHub.Core.Interfaces;

public interface IAuthRepository
{
    Task SaveChangesAsync(User user);
    Task<User?> GetUserByTokenAsync(string token);
}
