using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Models.Configurations;

public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.HasIndex(rc => new { rc.RoleId, rc.ClaimId }).IsUnique();
        builder.HasQueryFilter(rc => !rc.IsDeleted);
        builder.HasOne(rc => rc.Role).WithMany(r => r.RoleClaims).HasForeignKey(rc => rc.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(rc => rc.Claim).WithMany(c => c.RoleClaims).HasForeignKey(rc => rc.ClaimId).OnDelete(DeleteBehavior.Cascade);
    }
}
