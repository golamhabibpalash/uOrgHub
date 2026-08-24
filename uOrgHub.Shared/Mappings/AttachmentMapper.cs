using Riok.Mapperly.Abstractions;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Shared.Services.Attachments;

[Mapper]
public static partial class AttachmentMapper
{
    // The audit columns double as upload provenance: who added the file and when.
    [MapProperty(nameof(Attachment.CreatedBy), nameof(AttachmentDto.UploadedBy))]
    [MapProperty(nameof(Attachment.CreatedAt), nameof(AttachmentDto.UploadedAt))]
    [MapperIgnoreSource(nameof(Attachment.StorageKey))]
    [MapperIgnoreSource(nameof(Attachment.UpdatedAt))]
    [MapperIgnoreSource(nameof(Attachment.UpdatedBy))]
    [MapperIgnoreSource(nameof(Attachment.IsDeleted))]
    [MapperIgnoreSource(nameof(Attachment.DeletedAt))]
    [MapperIgnoreSource(nameof(Attachment.DeletedBy))]
    public static partial AttachmentDto ToDto(Attachment attachment);
}
