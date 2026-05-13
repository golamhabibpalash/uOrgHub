using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class OnboardingChecklistConfiguration : IEntityTypeConfiguration<OnboardingChecklist>
{
    public void Configure(EntityTypeBuilder<OnboardingChecklist> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Designation).WithMany()
         .HasForeignKey(x => x.DesignationId).OnDelete(DeleteBehavior.SetNull);
    }
}
