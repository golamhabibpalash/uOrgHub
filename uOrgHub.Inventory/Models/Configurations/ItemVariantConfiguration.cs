using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class ItemVariantConfiguration : IEntityTypeConfiguration<ItemVariant>
{
    public void Configure(EntityTypeBuilder<ItemVariant> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.SKU).IsUnique();

        b.HasOne(x => x.Item)
         .WithMany(x => x.Variants)
         .HasForeignKey(x => x.ItemId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
