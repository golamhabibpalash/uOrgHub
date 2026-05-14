using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ItemVariantId, x.WarehouseId }).IsUnique();

        b.HasOne(x => x.ItemVariant)
         .WithMany()
         .HasForeignKey(x => x.ItemVariantId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Warehouse)
         .WithMany()
         .HasForeignKey(x => x.WarehouseId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
