using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class EmployeeRosterConfiguration : IEntityTypeConfiguration<EmployeeRoster>
{
    public void Configure(EntityTypeBuilder<EmployeeRoster> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.EmployeeId, x.RosterDate }).IsUnique();
        b.HasOne(x => x.Employee).WithMany()
         .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Shift).WithMany(x => x.Rosters)
         .HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
    }
}
