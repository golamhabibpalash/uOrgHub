using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class RequestForQuotationConfiguration : IEntityTypeConfiguration<RequestForQuotation>
{
    public void Configure(EntityTypeBuilder<RequestForQuotation> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.RFQNumber).IsUnique();

        b.HasOne(x => x.PurchaseRequisition)
         .WithMany()
         .HasForeignKey(x => x.PRId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(x => x.Items)
         .WithOne(x => x.RequestForQuotation)
         .HasForeignKey(x => x.RFQId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Quotations)
         .WithOne(x => x.RequestForQuotation)
         .HasForeignKey(x => x.RFQId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
