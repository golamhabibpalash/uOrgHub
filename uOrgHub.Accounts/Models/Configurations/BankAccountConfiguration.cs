using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.AccountNumber).IsUnique();
        b.HasOne(x => x.ChartOfAccount).WithMany()
         .HasForeignKey(x => x.ChartOfAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
