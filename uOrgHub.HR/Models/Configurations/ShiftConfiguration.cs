using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.WorkSchedule).WithMany(x => x.Shifts)
         .HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Restrict);
    }
}
