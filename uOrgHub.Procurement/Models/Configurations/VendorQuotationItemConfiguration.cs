using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class VendorQuotationItemConfiguration : IEntityTypeConfiguration<VendorQuotationItem>
{
    public void Configure(EntityTypeBuilder<VendorQuotationItem> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.RFQItem)
         .WithMany()
         .HasForeignKey(x => x.RFQItemId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
