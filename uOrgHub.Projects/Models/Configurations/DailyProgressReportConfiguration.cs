using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class DailyProgressReportConfiguration : IEntityTypeConfiguration<DailyProgressReport>
{
    public void Configure(EntityTypeBuilder<DailyProgressReport> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ProjectId, x.ReportDate }).IsUnique();

        b.HasOne(x => x.Project)
         .WithMany(x => x.DPRs)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.WBS)
         .WithMany()
         .HasForeignKey(x => x.WBSId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
