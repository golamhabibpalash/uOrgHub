using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class AttendanceLogConfiguration : IEntityTypeConfiguration<AttendanceLog>
{
    public void Configure(EntityTypeBuilder<AttendanceLog> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.EmployeeId, x.AttendanceDate }).IsUnique();
        b.HasOne(x => x.Employee).WithMany(x => x.AttendanceLogs)
         .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
    }
}
