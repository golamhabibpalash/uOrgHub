using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class MaterialRequestConfiguration : IEntityTypeConfiguration<ProjectMaterialRequest>
{
    public void Configure(EntityTypeBuilder<ProjectMaterialRequest> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.RequestNumber).IsUnique();

        b.HasOne(x => x.Project)
         .WithMany(x => x.MaterialRequests)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.WBS)
         .WithMany()
         .HasForeignKey(x => x.WBSId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
