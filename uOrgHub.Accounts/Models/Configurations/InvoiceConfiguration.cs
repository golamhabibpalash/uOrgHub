using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.InvoiceNumber).IsUnique();
        b.HasOne(x => x.Customer).WithMany(x => x.Invoices)
         .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FiscalYear).WithMany()
         .HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CostCenter).WithMany()
         .HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.JournalEntry).WithMany()
         .HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.SetNull);
    }
}
