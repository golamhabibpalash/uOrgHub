using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class EmployeeOnboardingConfiguration : IEntityTypeConfiguration<EmployeeOnboarding>
{
    public void Configure(EntityTypeBuilder<EmployeeOnboarding> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Employee).WithMany()
         .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OnboardingChecklist).WithMany()
         .HasForeignKey(x => x.OnboardingChecklistId).OnDelete(DeleteBehavior.Restrict);
    }
}
