using Microsoft.Extensions.Caching.Memory;
using uOrgHub.Auth.Repositories;

namespace uOrgHub.Auth.Services;

public class PermissionService : IPermissionService
{
    private readonly IUserRepository _users;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public PermissionService(IUserRepository users, IMemoryCache cache)
    {
        _users = users;
        _cache = cache;
    }

    public async Task<bool> HasClaimAsync(Guid userId, string claimName)
    {
        var claims = await GetUserClaimsAsync(userId);
        return claims.Contains(claimName);
    }

    public async Task<List<string>> GetUserClaimsAsync(Guid userId)
    {
        var key = $"perm:claims:{userId}";
        if (_cache.TryGetValue(key, out List<string>? cached) && cached != null)
            return cached;

        var raw = await _users.GetUserClaimsAsync(userId);
        var granted = raw.Where(c => c.IsGranted).Select(c => c.Name).ToList();
        _cache.Set(key, granted, CacheDuration);
        return granted;
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var key = $"perm:roles:{userId}";
        if (_cache.TryGetValue(key, out List<string>? cached) && cached != null)
            return cached;

        var roles = (await _users.GetUserRolesAsync(userId)).ToList();
        _cache.Set(key, roles, CacheDuration);
        return roles;
    }

    public void InvalidateCache(Guid userId)
    {
        _cache.Remove($"perm:claims:{userId}");
        _cache.Remove($"perm:roles:{userId}");
    }
}
