using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class GoodsReceivedNoteConfiguration : IEntityTypeConfiguration<GoodsReceivedNote>
{
    public void Configure(EntityTypeBuilder<GoodsReceivedNote> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.GRNNumber).IsUnique();

        b.HasOne(x => x.PurchaseOrder)
         .WithMany(x => x.GoodsReceivedNotes)
         .HasForeignKey(x => x.POId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Items)
         .WithOne(x => x.GoodsReceivedNote)
         .HasForeignKey(x => x.GRNId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
