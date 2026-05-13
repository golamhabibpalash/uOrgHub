using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class OnboardingTaskProgressConfiguration : IEntityTypeConfiguration<OnboardingTaskProgress>
{
    public void Configure(EntityTypeBuilder<OnboardingTaskProgress> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.EmployeeOnboardingId, x.OnboardingTaskId }).IsUnique();
        b.HasOne(x => x.EmployeeOnboarding).WithMany(x => x.TaskProgresses)
         .HasForeignKey(x => x.EmployeeOnboardingId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.OnboardingTask).WithMany()
         .HasForeignKey(x => x.OnboardingTaskId).OnDelete(DeleteBehavior.Restrict);
    }
}
