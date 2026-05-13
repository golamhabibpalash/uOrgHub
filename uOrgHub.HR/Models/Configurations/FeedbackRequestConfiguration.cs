using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class FeedbackRequestConfiguration : IEntityTypeConfiguration<FeedbackRequest>
{
    public void Configure(EntityTypeBuilder<FeedbackRequest> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.PerformanceReview).WithMany(x => x.FeedbackRequests)
         .HasForeignKey(x => x.PerformanceReviewId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.RequestedFrom).WithMany()
         .HasForeignKey(x => x.RequestedFromId).OnDelete(DeleteBehavior.Restrict);
    }
}
