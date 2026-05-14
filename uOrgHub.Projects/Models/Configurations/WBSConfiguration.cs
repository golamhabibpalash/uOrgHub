using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class WBSConfiguration : IEntityTypeConfiguration<WorkBreakdownStructure>
{
    public void Configure(EntityTypeBuilder<WorkBreakdownStructure> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.CompletionPercent).HasColumnType("decimal(5,2)");

        b.HasOne(x => x.Project)
         .WithMany(x => x.WBSItems)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.ParentWBS)
         .WithMany(x => x.Children)
         .HasForeignKey(x => x.ParentWBSId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
