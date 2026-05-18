using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class ProjectSubmittalConfiguration : IEntityTypeConfiguration<ProjectSubmittal>
{
    public void Configure(EntityTypeBuilder<ProjectSubmittal> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ProjectId, x.SubmittalNumber }).IsUnique();

        b.HasOne(x => x.Project)
         .WithMany(x => x.Submittals)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
