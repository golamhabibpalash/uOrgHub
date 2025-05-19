using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities;
using OrgHub.Infrastructure.Persistence;

namespace OrgHub.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<User> _userManager;
        public AuthRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<User?> GetUserByTokenAsync(string token)
        {
            return await _userManager.Users
             .Include(x => x.RefreshTokens)
             .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token && t.Expires > DateTime.UtcNow));
        }

        public async Task SaveChangesAsync(User user)
        {
            await _userManager.UpdateAsync(user);
        }
    }
}
