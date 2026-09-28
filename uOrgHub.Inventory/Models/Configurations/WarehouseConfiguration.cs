using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Inventory.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Inventory.Models.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> b)
    {
        b.HasKey(x => x.Id);
        // Composite: two sister concerns can each run their own "Main"/"MW-01" warehouse code
        // independently — see SISTER_CONCERN_PLAN.md §6.
        b.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}
