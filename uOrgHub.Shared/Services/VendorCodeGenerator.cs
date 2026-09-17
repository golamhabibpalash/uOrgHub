using Microsoft.EntityFrameworkCore;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Shared.Services;

/// <summary>
/// One vendor-code sequence shared by both Accounts and Procurement's create-vendor handlers, now
/// that they operate on the same Vendor table — replaces the two modules' previously separate,
/// differently-prefixed generators (Accounts: VEND-{year}-####, Procurement: VND-####) so codes
/// minted from either creation path can never collide with each other or with legacy codes from
/// either old scheme. IgnoreQueryFilters() counts soft-deleted rows too, so a deleted vendor's slot
/// in the sequence is never reissued.
/// </summary>
public static class VendorCodeGenerator
{
    public static async Task<string> GenerateAsync(AppDbContext context, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await context.Set<Vendor>().IgnoreQueryFilters().CountAsync(ct) + 1;
        return $"VEND-{year}-{count:D4}";
    }
}
