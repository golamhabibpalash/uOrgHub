namespace uOrgHub.Shared.Services;

/// <summary>
/// Resolves the active sister concern for the current request from the JWT's "company_id" claim
/// (set at login/refresh/switch — see AuthService, JwtService). Null outside a request (seeders,
/// migrations, background work, tests) or for a user with no company assigned yet — AppDbContext
/// treats null as "don't filter" rather than "show nothing".
/// </summary>
public interface ICurrentCompanyAccessor
{
    Guid? CompanyId { get; }
}
