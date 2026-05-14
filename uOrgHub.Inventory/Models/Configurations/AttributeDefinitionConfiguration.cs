using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.Category)
         .WithMany()
         .HasForeignKey(x => x.CategoryId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
