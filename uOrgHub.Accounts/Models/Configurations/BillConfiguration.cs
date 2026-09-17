using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.BillNumber).IsUnique();
        // No inverse Vendor.Bills collection: Vendor now lives in uOrgHub.Shared, which cannot
        // reference uOrgHub.Accounts.Models.Entities.Bill without a circular project dependency.
        b.HasOne(x => x.Vendor).WithMany()
         .HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FiscalYear).WithMany()
         .HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CostCenter).WithMany()
         .HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.JournalEntry).WithMany()
         .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.SetNull);
    }
}
