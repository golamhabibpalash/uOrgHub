using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Shared.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasIndex(c => c.Name).IsUnique().HasFilter("\"IsDeleted\" = false");
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
