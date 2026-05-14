using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class InventoryCategoryConfiguration : IEntityTypeConfiguration<InventoryCategory>
{
    public void Configure(EntityTypeBuilder<InventoryCategory> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();

        b.HasOne(x => x.Type)
         .WithMany()
         .HasForeignKey(x => x.TypeId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ParentCategory)
         .WithMany(x => x.Children)
         .HasForeignKey(x => x.ParentCategoryId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
