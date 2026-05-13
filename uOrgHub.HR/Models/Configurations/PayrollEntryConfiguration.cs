using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class PayrollEntryConfiguration : IEntityTypeConfiguration<PayrollEntry>
{
    public void Configure(EntityTypeBuilder<PayrollEntry> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.PayrollCycleId, x.EmployeeId }).IsUnique();
        b.HasOne(x => x.PayrollCycle).WithMany(x => x.Entries)
         .HasForeignKey(x => x.PayrollCycleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Employee).WithMany(x => x.PayrollEntries)
         .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
