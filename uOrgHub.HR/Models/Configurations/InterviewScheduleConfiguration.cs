using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class InterviewScheduleConfiguration : IEntityTypeConfiguration<InterviewSchedule>
{
    public void Configure(EntityTypeBuilder<InterviewSchedule> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.JobApplication).WithMany(x => x.InterviewSchedules)
         .HasForeignKey(x => x.JobApplicationId).OnDelete(DeleteBehavior.Cascade);
    }
}
