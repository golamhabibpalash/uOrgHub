using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.Authorization;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;

namespace uOrgHub.Auth.Seeders;

public class AuthSeeder : IAuthSeeder
{
    private const string SeederActor = "system";
    private readonly AppDbContext _db;

    public AuthSeeder(AppDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var claims = await EnsureClaimsAsync(ct);
        var roles = await EnsureRolesAsync(ct);
        await EnsureRoleClaimsAsync(roles, claims, ct);
    }

    private async Task<Dictionary<string, ApplicationClaim>> EnsureClaimsAsync(CancellationToken ct)
    {
        var existing = await _db.Set<ApplicationClaim>()
            .Where(c => !c.IsDeleted)
            .ToDictionaryAsync(c => c.Name, ct);

        var added = false;
        foreach (var def in AuthorizationCatalog.AllClaims)
        {
            if (existing.ContainsKey(def.Name)) continue;

            var claim = new ApplicationClaim
            {
                Name = def.Name,
                Module = def.Module,
                Category = def.Category,
                Description = def.Description,
                IsActive = true,
                CreatedBy = SeederActor,
            };
            _db.Set<ApplicationClaim>().Add(claim);
            existing[def.Name] = claim;
            added = true;
        }

        if (added) await _db.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<Dictionary<string, ApplicationRole>> EnsureRolesAsync(CancellationToken ct)
    {
        var existing = await _db.Set<ApplicationRole>()
            .Where(r => !r.IsDeleted)
            .ToDictionaryAsync(r => r.Name, ct);

        var added = false;
        foreach (var def in AuthorizationCatalog.AllRoles)
        {
            if (existing.ContainsKey(def.Name)) continue;

            var role = new ApplicationRole
            {
                Name = def.Name,
                Description = def.Description,
                IsSystem = true,
                IsActive = true,
                CreatedBy = SeederActor,
            };
            _db.Set<ApplicationRole>().Add(role);
            existing[def.Name] = role;
            added = true;
        }

        if (added) await _db.SaveChangesAsync(ct);
        return existing;
    }

    private async Task EnsureRoleClaimsAsync(
        Dictionary<string, ApplicationRole> roles,
        Dictionary<string, ApplicationClaim> claims,
        CancellationToken ct)
    {
        var existing = await _db.Set<RoleClaim>()
            .Select(rc => new { rc.RoleId, rc.ClaimId })
            .ToListAsync(ct);
        var existingSet = existing.Select(x => (x.RoleId, x.ClaimId)).ToHashSet();

        var added = false;
        foreach (var def in AuthorizationCatalog.AllRoles)
        {
            if (!roles.TryGetValue(def.Name, out var role)) continue;
            foreach (var claimName in def.Claims)
            {
                if (!claims.TryGetValue(claimName, out var claim)) continue;
                if (existingSet.Contains((role.Id, claim.Id))) continue;

                _db.Set<RoleClaim>().Add(new RoleClaim
                {
                    RoleId = role.Id,
                    ClaimId = claim.Id,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = SeederActor,
                    CreatedBy = SeederActor,
                });
                added = true;
            }
        }

        if (added) await _db.SaveChangesAsync(ct);
    }
}
