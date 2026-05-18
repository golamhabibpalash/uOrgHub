using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class QAChecklistConfiguration : IEntityTypeConfiguration<QAChecklist>
{
    public void Configure(EntityTypeBuilder<QAChecklist> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ProjectId, x.ChecklistNumber }).IsUnique();

        b.HasOne(x => x.Project)
         .WithMany(x => x.QAChecklists)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.WBS)
         .WithMany()
         .HasForeignKey(x => x.WBSId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(x => x.Items)
         .WithOne(x => x.Checklist)
         .HasForeignKey(x => x.ChecklistId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QAChecklistItemConfiguration : IEntityTypeConfiguration<QAChecklistItem>
{
    public void Configure(EntityTypeBuilder<QAChecklistItem> b)
    {
        b.HasKey(x => x.Id);
    }
}
