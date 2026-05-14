using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.ItemCode).IsUnique().HasFilter("\"ItemCode\" IS NOT NULL");

        b.HasOne(x => x.Type)
         .WithMany()
         .HasForeignKey(x => x.TypeId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Category)
         .WithMany()
         .HasForeignKey(x => x.CategoryId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.UnitOfMeasure)
         .WithMany()
         .HasForeignKey(x => x.UnitOfMeasureId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
