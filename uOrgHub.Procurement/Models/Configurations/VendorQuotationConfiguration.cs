using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class VendorQuotationConfiguration : IEntityTypeConfiguration<VendorQuotation>
{
    public void Configure(EntityTypeBuilder<VendorQuotation> b)
    {
        b.HasKey(x => x.Id);
        // Composite: see PurchaseRequisitionConfiguration for why QuotationNumber can't stay
        // globally unique. (RFQId, VendorId) stays as-is — RFQId already pins this to one
        // company's RFQ, so it can't collide across companies on its own.
        b.HasIndex(x => new { x.CompanyId, x.QuotationNumber }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
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
