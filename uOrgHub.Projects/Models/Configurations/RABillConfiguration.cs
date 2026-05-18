using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class RABillConfiguration : IEntityTypeConfiguration<RABill>
{
    public void Configure(EntityTypeBuilder<RABill> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ProjectId, x.BillNumber }).IsUnique();

        b.Property(x => x.GrossAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.DeductionAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.RetentionPercent).HasColumnType("decimal(5,2)");
        b.Property(x => x.RetentionAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.PreviousBilledAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.CumulativeBilledAmount).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Project)
         .WithMany(x => x.RABills)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Items)
         .WithOne(x => x.RABill)
         .HasForeignKey(x => x.RABillId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RABillItemConfiguration : IEntityTypeConfiguration<RABillItem>
{
    public void Configure(EntityTypeBuilder<RABillItem> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.PreviousQuantity).HasColumnType("decimal(18,4)");
        b.Property(x => x.CurrentQuantity).HasColumnType("decimal(18,4)");
        b.Property(x => x.TotalQuantity).HasColumnType("decimal(18,4)");
        b.Property(x => x.Rate).HasColumnType("decimal(18,2)");
        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.BOQItem)
         .WithMany()
         .HasForeignKey(x => x.BOQItemId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
