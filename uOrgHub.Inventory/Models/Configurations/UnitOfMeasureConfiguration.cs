using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Abbreviation).IsUnique();
    }
}
