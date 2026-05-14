using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;

namespace uOrgHub.Auth.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _db;

    public RoleRepository(AppDbContext db) => _db = db;

    private IQueryable<ApplicationRole> BaseQuery() =>
        _db.Set<ApplicationRole>().Where(r => !r.IsDeleted);

    public async Task<List<ApplicationRole>> GetAllWithClaimsAsync() =>
        await BaseQuery()
            .Include(r => r.RoleClaims.Where(rc => !rc.IsDeleted)).ThenInclude(rc => rc.Claim)
            .OrderBy(r => r.Name)
            .ToListAsync();

    public async Task<ApplicationRole?> GetByIdAsync(Guid id) =>
        await BaseQuery().FirstOrDefaultAsync(r => r.Id == id);

    public async Task<ApplicationRole?> GetByIdWithClaimsAsync(Guid id) =>
        await BaseQuery()
            .Include(r => r.RoleClaims.Where(rc => !rc.IsDeleted)).ThenInclude(rc => rc.Claim)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<ApplicationRole?> GetByNameAsync(string name) =>
        await BaseQuery().FirstOrDefaultAsync(r => r.Name == name);

    public async Task<ApplicationRole> AddAsync(ApplicationRole role)
    {
        _db.Set<ApplicationRole>().Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task UpdateAsync(ApplicationRole role)
    {
        _db.Set<ApplicationRole>().Update(role);
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(ApplicationRole role, string deletedBy)
    {
        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        role.DeletedBy = deletedBy;
        await UpdateAsync(role);
    }

    public async Task SetRoleClaimsAsync(Guid roleId, List<Guid> claimIds, string assignedBy)
    {
        var existing = await _db.Set<RoleClaim>().Where(rc => rc.RoleId == roleId && !rc.IsDeleted).ToListAsync();
        existing.ForEach(rc => { rc.IsDeleted = true; rc.DeletedAt = DateTime.UtcNow; });

        var newClaims = claimIds.Select(cid => new RoleClaim
        {
            RoleId = roleId,
            ClaimId = cid,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
        });
        _db.Set<RoleClaim>().AddRange(newClaims);
        await _db.SaveChangesAsync();
    }

    public async Task AddRoleClaimAsync(Guid roleId, Guid claimId, string assignedBy)
    {
        var exists = await _db.Set<RoleClaim>()
            .AnyAsync(rc => rc.RoleId == roleId && rc.ClaimId == claimId && !rc.IsDeleted);
        if (exists) return;

        _db.Set<RoleClaim>().Add(new RoleClaim
        {
            RoleId = roleId,
            ClaimId = claimId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    public async Task RemoveRoleClaimAsync(Guid roleId, Guid claimId)
    {
        var rc = await _db.Set<RoleClaim>()
            .FirstOrDefaultAsync(rc => rc.RoleId == roleId && rc.ClaimId == claimId && !rc.IsDeleted);
        if (rc != null)
        {
            rc.IsDeleted = true;
            rc.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<int> GetUserCountAsync(Guid roleId) =>
        await _db.Set<UserRole>().CountAsync(ur => ur.RoleId == roleId && !ur.IsDeleted);
}
