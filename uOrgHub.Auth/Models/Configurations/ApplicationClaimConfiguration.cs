using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Models.Configurations;

public class ApplicationClaimConfiguration : IEntityTypeConfiguration<ApplicationClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationClaim> builder)
    {
        builder.HasIndex(c => c.Name).IsUnique().HasFilter("\"IsDeleted\" = false");
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
