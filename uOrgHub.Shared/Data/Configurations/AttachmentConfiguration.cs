using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Shared.Data.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        // The common lookup: all attachments of one record, newest first.
        builder.HasIndex(a => new { a.EntityType, a.EntityId });

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
