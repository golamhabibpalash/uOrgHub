using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
{
    public void Configure(EntityTypeBuilder<BankTransaction> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.BankAccount).WithMany(x => x.Transactions)
         .HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.JournalEntry).WithMany()
         .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.SetNull);
    }
}
