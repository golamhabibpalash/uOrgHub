using OrgHub.Domain.Entities;

namespace OrgHub.Core.Interfaces;

public interface IAuthRepository
{
    Task SaveChangesAsync(User user);
    Task<User?> GetUserByTokenAsync(string token);
}
