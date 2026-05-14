using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.VendorCode).IsUnique();
        b.HasOne(x => x.PayableAccount).WithMany()
         .HasForeignKey(x => x.PayableAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
