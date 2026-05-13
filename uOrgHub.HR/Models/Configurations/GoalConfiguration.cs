using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Employee).WithMany()
         .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ReviewCycle).WithMany(x => x.Goals)
         .HasForeignKey(x => x.ReviewCycleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.KPI).WithMany(x => x.Goals)
         .HasForeignKey(x => x.KPIId).OnDelete(DeleteBehavior.SetNull);
    }
}
