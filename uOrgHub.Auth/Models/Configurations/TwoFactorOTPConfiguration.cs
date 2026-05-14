using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Auth.Models.Entities;

namespace uOrgHub.Auth.Models.Configurations;

public class TwoFactorOTPConfiguration : IEntityTypeConfiguration<TwoFactorOTP>
{
    public void Configure(EntityTypeBuilder<TwoFactorOTP> builder)
    {
        builder.HasKey(o => o.Id);
        builder.HasOne(o => o.User).WithMany(u => u.TwoFactorOTPs).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(o => new { o.UserId, o.OTPType, o.IsUsed });
    }
}
