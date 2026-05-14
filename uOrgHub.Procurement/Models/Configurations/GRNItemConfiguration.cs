using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class GRNItemConfiguration : IEntityTypeConfiguration<GRNItem>
{
    public void Configure(EntityTypeBuilder<GRNItem> b)
    {
        b.HasKey(x => x.Id);

        b.HasOne(x => x.POItem)
         .WithMany()
         .HasForeignKey(x => x.POItemId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
