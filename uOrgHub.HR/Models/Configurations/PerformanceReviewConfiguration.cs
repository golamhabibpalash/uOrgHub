using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    public void Configure(EntityTypeBuilder<PerformanceReview> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.ReviewCycle).WithMany(x => x.PerformanceReviews)
         .HasForeignKey(x => x.ReviewCycleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Employee).WithMany()
         .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Reviewer).WithMany()
         .HasForeignKey(x => x.ReviewerId).OnDelete(DeleteBehavior.Restrict);
    }
}
