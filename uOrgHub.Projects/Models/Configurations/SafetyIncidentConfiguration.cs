using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class SafetyIncidentConfiguration : IEntityTypeConfiguration<SafetyIncident>
{
    public void Configure(EntityTypeBuilder<SafetyIncident> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.ProjectId, x.IncidentNumber }).IsUnique();

        b.HasOne(x => x.Project)
         .WithMany(x => x.SafetyIncidents)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
