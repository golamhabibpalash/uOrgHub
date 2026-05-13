using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.CandidateId, x.JobPostingId }).IsUnique();
        b.HasOne(x => x.Candidate).WithMany(x => x.Applications)
         .HasForeignKey(x => x.CandidateId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.JobPosting).WithMany(x => x.Applications)
         .HasForeignKey(x => x.JobPostingId).OnDelete(DeleteBehavior.Restrict);
    }
}
