using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Entities;
using uOrgHub.Shared.Models.Enums;
using uOrgHub.Shared.Services;

namespace uOrgHub.API.Services;

/// <summary>
/// Resolves a customer (Accounts), vendor (Shared) or client (Projects) to its name and phone.
/// Here because uOrgHub.API is the only project that references every module.
/// </summary>
public class PartyContactDirectory(AppDbContext db) : IPartyContactDirectory
{
    public Task<PartyContact?> FindAsync(PartyType type, Guid id, CancellationToken ct = default) => type switch
    {
        PartyType.Customer => db.Set<Customer>().AsNoTracking().Where(c => !c.IsDeleted && c.Id == id)
            .Select(c => new PartyContact(PartyType.Customer, c.Id, c.Name, c.Phone)).FirstOrDefaultAsync(ct),
        PartyType.Vendor => db.Set<Vendor>().AsNoTracking().Where(v => !v.IsDeleted && v.Id == id)
            .Select(v => new PartyContact(PartyType.Vendor, v.Id, v.Name, v.Phone)).FirstOrDefaultAsync(ct),
        PartyType.Client => db.Set<Client>().AsNoTracking().Where(c => !c.IsDeleted && c.Id == id)
            .Select(c => new PartyContact(PartyType.Client, c.Id, c.CompanyName, c.Phone)).FirstOrDefaultAsync(ct),
        _ => Task.FromResult<PartyContact?>(null),
    };
}
