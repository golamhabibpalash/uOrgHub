namespace uOrgHub.Auth.Services;

public interface IPermissionService
{
    Task<bool> HasClaimAsync(Guid userId, string claimName);
    Task<List<string>> GetUserClaimsAsync(Guid userId);
    Task<List<string>> GetUserRolesAsync(Guid userId);
    void InvalidateCache(Guid userId);
}
