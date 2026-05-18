using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class NonConformanceReportConfiguration : IEntityTypeConfiguration<NonConformanceReport>
{
    public void Configure(EntityTypeBuilder<NonConformanceReport> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ProjectId, x.NCRNumber }).IsUnique();

        b.HasOne(x => x.Project)
         .WithMany(x => x.NCRs)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.WBS)
         .WithMany()
         .HasForeignKey(x => x.WBSId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
