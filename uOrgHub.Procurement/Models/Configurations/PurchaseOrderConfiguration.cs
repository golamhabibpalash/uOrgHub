using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.PONumber).IsUnique();

        b.HasOne(x => x.Vendor)
         .WithMany()
         .HasForeignKey(x => x.VendorId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Quotation)
         .WithMany()
         .HasForeignKey(x => x.QuotationId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasOne(x => x.PurchaseRequisition)
         .WithMany()
         .HasForeignKey(x => x.PRId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(x => x.Items)
         .WithOne(x => x.PurchaseOrder)
         .HasForeignKey(x => x.POId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.GoodsReceivedNotes)
         .WithOne(x => x.PurchaseOrder)
         .HasForeignKey(x => x.POId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
