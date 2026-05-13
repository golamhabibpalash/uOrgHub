using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.JobCode).IsUnique();
        b.HasOne(x => x.Department).WithMany()
         .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Designation).WithMany()
         .HasForeignKey(x => x.DesignationId).OnDelete(DeleteBehavior.Restrict);
    }
}
