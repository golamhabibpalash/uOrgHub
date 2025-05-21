using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrgHub.Core.Interfaces;
using OrgHub.Domain.Entities.Identity;

namespace OrgHub.Infrastructure.Repositories.Identity
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AuthRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<ApplicationUser?> GetUserByTokenAsync(string token)
        {
            return await _userManager.Users
             .Include(x => x.RefreshTokens)
             .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token && t.Expires > DateTime.UtcNow));
        }

        public async Task SaveChangesAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
        }
    }
}
