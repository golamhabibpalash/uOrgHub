using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class ProjectStatusLogConfiguration : IEntityTypeConfiguration<ProjectStatusLog>
{
    public void Configure(EntityTypeBuilder<ProjectStatusLog> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.FromStatus)
         .HasConversion<string>()
         .HasMaxLength(20);

        b.Property(x => x.ToStatus)
         .HasConversion<string>()
         .HasMaxLength(20);

        b.HasOne(x => x.Project)
         .WithMany(x => x.StatusLogs)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
