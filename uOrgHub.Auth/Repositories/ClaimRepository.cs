using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.Shared.Data;

namespace uOrgHub.Auth.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly AppDbContext _db;

    public ClaimRepository(AppDbContext db) => _db = db;

    private IQueryable<ApplicationClaim> BaseQuery() =>
        _db.Set<ApplicationClaim>().Where(c => !c.IsDeleted);

    public async Task<List<ApplicationClaim>> GetAllAsync(string? module = null)
    {
        var query = BaseQuery();
        if (!string.IsNullOrWhiteSpace(module))
            query = query.Where(c => c.Module == module);
        return await query.OrderBy(c => c.Module).ThenBy(c => c.Name).ToListAsync();
    }

    public async Task<ApplicationClaim?> GetByIdAsync(Guid id) =>
        await BaseQuery().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ApplicationClaim?> GetByNameAsync(string name) =>
        await BaseQuery().FirstOrDefaultAsync(c => c.Name == name);

    public async Task<ApplicationClaim> AddAsync(ApplicationClaim claim)
    {
        _db.Set<ApplicationClaim>().Add(claim);
        await _db.SaveChangesAsync();
        return claim;
    }

    public async Task UpdateAsync(ApplicationClaim claim)
    {
        _db.Set<ApplicationClaim>().Update(claim);
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(ApplicationClaim claim, string deletedBy)
    {
        claim.IsDeleted = true;
        claim.DeletedAt = DateTime.UtcNow;
        claim.DeletedBy = deletedBy;
        await UpdateAsync(claim);
    }
}
