using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Models.Configurations;

public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.HasIndex(uc => new { uc.UserId, uc.ClaimId }).IsUnique();
        builder.HasQueryFilter(uc => !uc.IsDeleted);
        builder.HasOne(uc => uc.User).WithMany(u => u.UserClaims).HasForeignKey(uc => uc.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(uc => uc.Claim).WithMany(c => c.UserClaims).HasForeignKey(uc => uc.ClaimId).OnDelete(DeleteBehavior.Cascade);
    }
}
