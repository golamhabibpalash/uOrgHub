using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Models;

namespace uOrgHub.Auth.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    private IQueryable<ApplicationUser> BaseQuery() =>
        _db.Set<ApplicationUser>().Where(u => !u.IsDeleted);

    public async Task<ApplicationUser?> GetByUsernameAsync(string username) =>
        await BaseQuery().FirstOrDefaultAsync(u => u.Username == username);

    public async Task<ApplicationUser?> GetByEmailAsync(string email) =>
        await BaseQuery().FirstOrDefaultAsync(u => u.Email == email);

    public async Task<ApplicationUser?> GetByIdAsync(Guid id) =>
        await BaseQuery().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<ApplicationUser?> GetByIdWithDetailsAsync(Guid id) =>
        await BaseQuery()
            .Include(u => u.UserRoles.Where(ur => !ur.IsDeleted)).ThenInclude(ur => ur.Role)
            .Include(u => u.UserClaims.Where(uc => !uc.IsDeleted)).ThenInclude(uc => uc.Claim)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<PagedResult<ApplicationUser>> GetPagedAsync(int page, int pageSize, string? search)
    {
        IQueryable<ApplicationUser> query = BaseQuery()
            .Include(u => u.UserRoles.Where(ur => !ur.IsDeleted)).ThenInclude(ur => ur.Role);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Username.Contains(search) || u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderBy(u => u.FirstName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ApplicationUser> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<ApplicationUser> AddAsync(ApplicationUser user)
    {
        _db.Set<ApplicationUser>().Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        _db.Set<ApplicationUser>().Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(Guid userId) =>
        await _db.Set<UserRole>()
            .Where(ur => ur.UserId == userId && !ur.IsDeleted && !ur.Role.IsDeleted)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

    public async Task<IEnumerable<(string Name, bool IsGranted)>> GetUserClaimsAsync(Guid userId)
    {
        // Direct user claims (overrides/denies)
        var userClaims = await _db.Set<UserClaim>()
            .Where(uc => uc.UserId == userId && !uc.IsDeleted && !uc.Claim.IsDeleted && uc.Claim.IsActive)
            .Select(uc => new { uc.Claim.Name, uc.IsGranted })
            .ToListAsync();

        // Role-based claims
        var roleClaims = await _db.Set<UserRole>()
            .Where(ur => ur.UserId == userId && !ur.IsDeleted && !ur.Role.IsDeleted)
            .SelectMany(ur => ur.Role.RoleClaims.Where(rc => !rc.IsDeleted && !rc.Claim.IsDeleted && rc.Claim.IsActive))
            .Select(rc => rc.Claim.Name)
            .Distinct()
            .ToListAsync();

        // User-specific overrides take priority
        var deniedSet = userClaims.Where(uc => !uc.IsGranted).Select(uc => uc.Name).ToHashSet();
        var grantedOverrides = userClaims.Where(uc => uc.IsGranted).Select(uc => uc.Name).ToHashSet();

        var result = roleClaims
            .Where(c => !deniedSet.Contains(c))
            .Union(grantedOverrides)
            .Select(c => (c, true));

        return result;
    }

    public async Task SetUserRolesAsync(Guid userId, List<Guid> roleIds, string assignedBy)
    {
        var existing = await _db.Set<UserRole>().Where(ur => ur.UserId == userId && !ur.IsDeleted).ToListAsync();
        existing.ForEach(ur => { ur.IsDeleted = true; ur.DeletedAt = DateTime.UtcNow; });

        var newRoles = roleIds.Select(rid => new UserRole
        {
            UserId = userId,
            RoleId = rid,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
        });
        _db.Set<UserRole>().AddRange(newRoles);
        await _db.SaveChangesAsync();
    }

    public async Task AddUserClaimAsync(Guid userId, Guid claimId, bool isGranted, string assignedBy)
    {
        var existing = await _db.Set<UserClaim>()
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClaimId == claimId && !uc.IsDeleted);

        if (existing != null)
        {
            existing.IsGranted = isGranted;
            existing.AssignedBy = assignedBy;
        }
        else
        {
            _db.Set<UserClaim>().Add(new UserClaim
            {
                UserId = userId,
                ClaimId = claimId,
                IsGranted = isGranted,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.UtcNow,
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task RemoveUserClaimAsync(Guid userId, Guid claimId)
    {
        var claim = await _db.Set<UserClaim>()
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClaimId == claimId && !uc.IsDeleted);
        if (claim != null)
        {
            claim.IsDeleted = true;
            claim.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
