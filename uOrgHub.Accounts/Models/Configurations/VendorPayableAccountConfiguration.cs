using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

/// <summary>
/// Configures Vendor.PayableAccountId → ChartOfAccount from this side rather than on Vendor itself:
/// Vendor lives in uOrgHub.Shared, which cannot reference uOrgHub.Accounts.Models.Entities.ChartOfAccount
/// without a circular project dependency, so Vendor only holds the bare FK (no navigation property).
/// EF Core's HasOne&lt;T&gt;() lets this relationship still be configured from here, by FK alone.
/// </summary>
public class VendorPayableAccountConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> b)
    {
        b.HasOne<ChartOfAccount>().WithMany()
         .HasForeignKey(x => x.PayableAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
