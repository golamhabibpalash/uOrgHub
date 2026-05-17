using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Models.Configurations;

public class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
{
    public void Configure(EntityTypeBuilder<UserCompany> builder)
    {
        builder.HasIndex(uc => new { uc.UserId, uc.CompanyId }).IsUnique();
        builder.HasQueryFilter(uc => !uc.IsDeleted);
        builder.HasOne(uc => uc.User).WithMany(u => u.UserCompanies).HasForeignKey(uc => uc.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(uc => uc.Company).WithMany().HasForeignKey(uc => uc.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}
