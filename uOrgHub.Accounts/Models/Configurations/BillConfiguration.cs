using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> b)
    {
        b.HasKey(x => x.Id);
        // Composite, not a bare unique index on BillNumber: two sister concerns each generate
        // their own numbering (SISTER_CONCERN_PLAN.md §7) and can legitimately produce the same
        // number independently.
        b.HasIndex(x => new { x.CompanyId, x.BillNumber }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
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
