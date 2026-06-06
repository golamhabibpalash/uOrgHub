using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Services;

namespace uOrgHub.API.Services;

public class UserProfilePictureResolver : IUserProfilePictureResolver
{
    private readonly AppDbContext _db;

    public UserProfilePictureResolver(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string?> ResolveAsync(Guid employeeId)
    {
        return await _db.Set<Employee>()
            .Where(e => e.Id == employeeId)
            .Select(e => e.ProfilePicturePath)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<Guid, string?>> ResolveBatchAsync(IEnumerable<Guid> employeeIds)
    {
        return await _db.Set<Employee>()
            .Where(e => employeeIds.Contains(e.Id))
            .Select(e => new { e.Id, e.ProfilePicturePath })
            .ToDictionaryAsync(e => e.Id, e => e.ProfilePicturePath);
    }
}
