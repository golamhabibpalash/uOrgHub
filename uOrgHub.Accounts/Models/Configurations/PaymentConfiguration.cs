using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.HasKey(x => x.Id);
        // Composite: see BillConfiguration for why PaymentNumber can't stay globally unique.
        b.HasIndex(x => new { x.CompanyId, x.PaymentNumber }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Customer).WithMany(x => x.Payments)
         .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
        // No inverse Vendor.Payments collection — see BillConfiguration for why.
        b.HasOne(x => x.Vendor).WithMany()
         .HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.BankAccount).WithMany()
         .HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.FiscalYear).WithMany()
         .HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.JournalEntry).WithMany()
         .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.SetNull);
    }
}
