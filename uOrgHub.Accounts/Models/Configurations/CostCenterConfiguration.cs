using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> b)
    {
        b.HasKey(x => x.Id);
        // Composite: see BillConfiguration for why Code can't stay globally unique — two sister
        // concerns can each run their own "HO" or "SITE-1" cost center code independently.
        b.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ParentCostCenter).WithMany(x => x.Children)
         .HasForeignKey(x => x.ParentCostCenterId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.ProjectId);
    }
}
