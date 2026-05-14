using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class VariantAttributeConfiguration : IEntityTypeConfiguration<VariantAttribute>
{
    public void Configure(EntityTypeBuilder<VariantAttribute> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.ItemVariant)
         .WithMany(x => x.Attributes)
         .HasForeignKey(x => x.ItemVariantId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.AttributeDefinition)
         .WithMany()
         .HasForeignKey(x => x.AttributeDefinitionId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
