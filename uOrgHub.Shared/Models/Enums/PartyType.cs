namespace uOrgHub.Shared.Models.Enums;

/// <summary>
/// Kind of external party a document can name. The records live in different modules — Customer
/// in Accounts, Vendor in Shared, Client in Projects — so a document refers to one with this type
/// plus a bare id, and <see cref="Services.IPartyContactDirectory"/> resolves it.
/// </summary>
public enum PartyType
{
    Customer,
    Vendor,
    Client,
}
