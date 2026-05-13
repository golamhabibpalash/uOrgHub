using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class OnboardingTaskConfiguration : IEntityTypeConfiguration<OnboardingTask>
{
    public void Configure(EntityTypeBuilder<OnboardingTask> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.OnboardingChecklist).WithMany(x => x.Tasks)
         .HasForeignKey(x => x.OnboardingChecklistId).OnDelete(DeleteBehavior.Cascade);
    }
}
