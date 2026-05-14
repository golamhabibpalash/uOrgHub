using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Payment).WithMany(x => x.Allocations)
         .HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Invoice).WithMany(x => x.Allocations)
         .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Bill).WithMany(x => x.Allocations)
         .HasForeignKey(x => x.BillId).OnDelete(DeleteBehavior.SetNull);
    }
}
