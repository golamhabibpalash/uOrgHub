using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
        b.HasOne(x => x.ParentCostCenter).WithMany(x => x.Children)
         .HasForeignKey(x => x.ParentCostCenterId).OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.ProjectId);
    }
}
