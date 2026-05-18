using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class SiteResourceAllocationConfiguration : IEntityTypeConfiguration<SiteResourceAllocation>
{
    public void Configure(EntityTypeBuilder<SiteResourceAllocation> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.PlannedQuantity).HasColumnType("decimal(18,4)");
        b.Property(x => x.ActualQuantity).HasColumnType("decimal(18,4)");
        b.Property(x => x.UnitCost).HasColumnType("decimal(18,2)");
        b.Property(x => x.PlannedCost).HasColumnType("decimal(18,2)");
        b.Property(x => x.ActualCost).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Project)
         .WithMany(x => x.ResourceAllocations)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.WBS)
         .WithMany()
         .HasForeignKey(x => x.WBSId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
