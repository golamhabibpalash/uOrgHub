using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> b)
    {
        b.HasKey(x => x.Id);
        // Composite: two sister concerns number stock transactions independently (both
        // generators count only their own company's rows for the month, once StockTransaction
        // implements ICompanyScoped — SISTER_CONCERN_PLAN.md §6) and can legitimately collide on
        // TXN-{yyyyMM}-#####. This entity carries its own CompanyId rather than inheriting
        // through WarehouseId — see the marker interface comment on Warehouse.cs for why.
        b.HasIndex(x => new { x.CompanyId, x.TransactionNumber }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);

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
