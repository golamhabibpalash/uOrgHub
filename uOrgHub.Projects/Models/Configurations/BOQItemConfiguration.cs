using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class BOQItemConfiguration : IEntityTypeConfiguration<BOQItem>
{
    public void Configure(EntityTypeBuilder<BOQItem> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.EstimatedQuantity).HasColumnType("decimal(18,4)");
        b.Property(x => x.UnitRate).HasColumnType("decimal(18,2)");
        b.Property(x => x.EstimatedAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.ActualQuantity).HasColumnType("decimal(18,4)");
        b.Property(x => x.ActualAmount).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.BOQ)
         .WithMany(x => x.Items)
         .HasForeignKey(x => x.BOQId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
