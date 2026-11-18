using uOrgHub.Shared.Models.Enums;

namespace uOrgHub.Shared.Services;

/// <summary>A party's name and phone as held on its own record.</summary>
public record PartyContact(PartyType Type, Guid Id, string Name, string? Phone);

/// <summary>
/// Looks up a customer, vendor or client by type and id. Implemented in uOrgHub.API, the one
/// project that sees every module, so a module can resolve a party from another module without
/// referencing it.
/// </summary>
public interface IPartyContactDirectory
{
    Task<PartyContact?> FindAsync(PartyType type, Guid id, CancellationToken ct = default);
}
