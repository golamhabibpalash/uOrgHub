using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.PaymentNumber).IsUnique();
        b.HasOne(x => x.Customer).WithMany(x => x.Payments)
         .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Vendor).WithMany(x => x.Payments)
         .HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.BankAccount).WithMany()
         .HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.FiscalYear).WithMany()
         .HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.JournalEntry).WithMany()
         .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.SetNull);
    }
}
