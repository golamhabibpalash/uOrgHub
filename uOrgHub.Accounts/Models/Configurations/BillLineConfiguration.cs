using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class BillLineConfiguration : IEntityTypeConfiguration<BillLine>
{
    public void Configure(EntityTypeBuilder<BillLine> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Bill).WithMany(x => x.Lines)
         .HasForeignKey(x => x.BillId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.TaxRate).WithMany()
         .HasForeignKey(x => x.TaxRateId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.ExpenseAccount).WithMany()
         .HasForeignKey(x => x.ExpenseAccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CostCenter).WithMany(x => x.BillLines)
         .HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.SetNull);
    }
}
