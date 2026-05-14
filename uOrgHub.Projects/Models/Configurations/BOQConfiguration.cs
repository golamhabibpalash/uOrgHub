using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class BOQConfiguration : IEntityTypeConfiguration<BillOfQuantity>
{
    public void Configure(EntityTypeBuilder<BillOfQuantity> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.BOQNumber).IsUnique();

        b.Property(x => x.TotalEstimatedCost).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Project)
         .WithMany(x => x.BOQs)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.WBS)
         .WithMany()
         .HasForeignKey(x => x.WBSId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
