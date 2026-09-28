using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.HasKey(x => x.Id);
        // Composite: two sister concerns number projects independently (the generator counts
        // only its own company's rows for the year — SISTER_CONCERN_PLAN.md §6) and can
        // legitimately collide on PRJ-{year}-####.
        b.HasIndex(x => new { x.CompanyId, x.ProjectCode }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.ContractValue).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Client)
         .WithMany(x => x.Projects)
         .HasForeignKey(x => x.ClientId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Category)
         .WithMany(x => x.Projects)
         .HasForeignKey(x => x.CategoryId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
