using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Invoice).WithMany(x => x.Lines)
         .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.TaxRate).WithMany()
         .HasForeignKey(x => x.TaxRateId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.RevenueAccount).WithMany()
         .HasForeignKey(x => x.RevenueAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CostCenter).WithMany(x => x.InvoiceLines)
         .HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.SetNull);
    }
}
