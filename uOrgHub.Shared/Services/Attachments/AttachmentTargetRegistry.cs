namespace uOrgHub.Shared.Services.Attachments;

public class AttachmentTargetRegistry : IAttachmentTargetRegistry
{
    private readonly Dictionary<string, AttachmentTargetDefinition> _targets =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(AttachmentTargetDefinition definition)
    {
        _targets[definition.EntityType] = definition;
    }

    /// <exception cref="Shared.Exceptions.AppException">When the type was never registered.</exception>
    public AttachmentTargetDefinition Resolve(string entityType)
    {
        if (_targets.TryGetValue(entityType, out var definition))
            return definition;

        throw new Shared.Exceptions.AppException($"'{entityType}' does not accept attachments.", 404);
    }
}
