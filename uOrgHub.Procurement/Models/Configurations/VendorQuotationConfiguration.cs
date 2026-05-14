using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class VendorQuotationConfiguration : IEntityTypeConfiguration<VendorQuotation>
{
    public void Configure(EntityTypeBuilder<VendorQuotation> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.QuotationNumber).IsUnique();
        b.HasIndex(x => new { x.RFQId, x.VendorId }).IsUnique();

        b.HasOne(x => x.RequestForQuotation)
         .WithMany(x => x.Quotations)
         .HasForeignKey(x => x.RFQId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Vendor)
         .WithMany()
         .HasForeignKey(x => x.VendorId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Items)
         .WithOne(x => x.VendorQuotation)
         .HasForeignKey(x => x.QuotationId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
