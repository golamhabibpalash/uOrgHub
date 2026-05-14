using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class InventoryTypeConfiguration : IEntityTypeConfiguration<InventoryType>
{
    public void Configure(EntityTypeBuilder<InventoryType> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
    }
}
