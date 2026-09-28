using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> b)
    {
        b.HasKey(x => x.Id);
        // Composite: see BillConfiguration — two sister concerns can bank at the same account
        // number/branch in principle, and each company's list must not collide on it either way.
        b.HasIndex(x => new { x.CompanyId, x.AccountNumber }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ChartOfAccount).WithMany()
         .HasForeignKey(x => x.ChartOfAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
