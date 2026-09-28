using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class PurchaseRequisitionConfiguration : IEntityTypeConfiguration<PurchaseRequisition>
{
    public void Configure(EntityTypeBuilder<PurchaseRequisition> b)
    {
        b.HasKey(x => x.Id);
        // Composite: two sister concerns number PRs independently (each generator counts only
        // its own company's rows for the year — SISTER_CONCERN_PLAN.md §6) and can legitimately
        // collide on PR-{year}-####.
        b.HasIndex(x => new { x.CompanyId, x.PRNumber }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Items)
         .WithOne(x => x.PurchaseRequisition)
         .HasForeignKey(x => x.PRId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
