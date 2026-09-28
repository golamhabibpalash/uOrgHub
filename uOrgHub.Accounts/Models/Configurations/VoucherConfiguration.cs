using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> b)
    {
        b.HasKey(x => x.Id);
        // Composite: see BillConfiguration for why VoucherNumber can't stay globally unique.
        b.HasIndex(x => new { x.CompanyId, x.VoucherNumber }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.ProjectId);
        b.HasOne(x => x.CostCenter).WithMany()
            .HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FiscalYear).WithMany()
            .HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.DebitAccount).WithMany()
            .HasForeignKey(x => x.DebitAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreditAccount).WithMany()
            .HasForeignKey(x => x.CreditAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.JournalEntry).WithMany()
            .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.SetNull);
    }
}