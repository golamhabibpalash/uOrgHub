using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Models.Configurations;

public class UserAccessLogConfiguration : IEntityTypeConfiguration<UserAccessLog>
{
    public void Configure(EntityTypeBuilder<UserAccessLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => new { l.UserId, l.CreatedAt });
        builder.HasIndex(l => l.CreatedAt);
        builder.HasOne(l => l.User).WithMany(u => u.AccessLogs).HasForeignKey(l => l.UserId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
    }
}
