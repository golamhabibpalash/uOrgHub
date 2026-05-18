using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class ProjectRFIConfiguration : IEntityTypeConfiguration<ProjectRFI>
{
    public void Configure(EntityTypeBuilder<ProjectRFI> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ProjectId, x.RFINumber }).IsUnique();

        b.HasOne(x => x.Project)
         .WithMany(x => x.RFIs)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.WBS)
         .WithMany()
         .HasForeignKey(x => x.WBSId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
