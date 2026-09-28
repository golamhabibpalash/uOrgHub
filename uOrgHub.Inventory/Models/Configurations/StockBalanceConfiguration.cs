using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> b)
    {
        b.HasKey(x => x.Id);
        // (ItemVariantId, WarehouseId) stays as-is, not composited with CompanyId: WarehouseId
        // already pins a balance to one company's warehouse, so it can't collide across
        // companies on its own. CompanyId itself is still stamped/filtered like every other
        // ICompanyScoped entity — see StockTransactionConfiguration for why this needed its own
        // column rather than inheriting scoping through Warehouse.
        b.HasIndex(x => new { x.ItemVariantId, x.WarehouseId }).IsUnique();
        b.HasIndex(x => x.CompanyId);
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);

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
