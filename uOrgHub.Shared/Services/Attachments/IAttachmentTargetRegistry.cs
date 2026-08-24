namespace uOrgHub.Shared.Services.Attachments;

/// <summary>
/// Declares one kind of owning record that accepts attachments. The claims decide who may see and
/// who may add/remove attachments on that record: attachment access always follows the parent
/// record's own permissions rather than introducing a separate permission set.
/// </summary>
public class AttachmentTargetDefinition
{
    /// <summary>Short name stored on every attachment row, e.g. "Voucher".</summary>
    public required string EntityType { get; init; }

    /// <summary>Claim needed to list/download attachments of this type.</summary>
    public required string ViewClaim { get; init; }

    /// <summary>Claim needed to upload or delete attachments of this type.</summary>
    public required string EditClaim { get; init; }
}

/// <summary>
/// Knows which record types accept attachments and what each one demands of the caller. Every
/// module registers its targets at startup; an unregistered type simply cannot be attached to,
/// so adding attachment support to a new module is a single registration line.
/// </summary>
public interface IAttachmentTargetRegistry
{
    void Register(AttachmentTargetDefinition definition);

    AttachmentTargetDefinition Resolve(string entityType);
}
