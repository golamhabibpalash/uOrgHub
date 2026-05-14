using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class MaterialRequestItemConfiguration : IEntityTypeConfiguration<ProjectMaterialRequestItem>
{
    public void Configure(EntityTypeBuilder<ProjectMaterialRequestItem> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.RequestedQuantity).HasColumnType("decimal(18,4)");
        b.Property(x => x.ApprovedQuantity).HasColumnType("decimal(18,4)");
        b.Property(x => x.IssuedQuantity).HasColumnType("decimal(18,4)");

        b.HasOne(x => x.Request)
         .WithMany(x => x.Items)
         .HasForeignKey(x => x.RequestId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.BOQItem)
         .WithMany()
         .HasForeignKey(x => x.BOQItemId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
