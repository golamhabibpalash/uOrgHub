using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.TransactionNumber).IsUnique();

        b.HasOne(x => x.ItemVariant)
         .WithMany()
         .HasForeignKey(x => x.ItemVariantId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Warehouse)
         .WithMany()
         .HasForeignKey(x => x.WarehouseId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.FromWarehouse)
         .WithMany()
         .HasForeignKey(x => x.FromWarehouseId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
