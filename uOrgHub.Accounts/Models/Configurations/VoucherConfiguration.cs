using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.VoucherNumber).IsUnique();
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