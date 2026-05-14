using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class PurchaseRequisitionConfiguration : IEntityTypeConfiguration<PurchaseRequisition>
{
    public void Configure(EntityTypeBuilder<PurchaseRequisition> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.PRNumber).IsUnique();

        b.HasMany(x => x.Items)
         .WithOne(x => x.PurchaseRequisition)
         .HasForeignKey(x => x.PRId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
