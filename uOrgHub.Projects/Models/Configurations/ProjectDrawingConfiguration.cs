using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class ProjectDrawingConfiguration : IEntityTypeConfiguration<ProjectDrawing>
{
    public void Configure(EntityTypeBuilder<ProjectDrawing> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ProjectId, x.DrawingNumber, x.Revision }).IsUnique();

        b.HasOne(x => x.Project)
         .WithMany(x => x.Drawings)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.WBS)
         .WithMany()
         .HasForeignKey(x => x.WBSId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
