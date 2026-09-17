using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Shared.Data.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.HasIndex(v => v.VendorCode).IsUnique();
        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
