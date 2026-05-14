using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.FiscalYear).WithMany()
         .HasForeignKey(x => x.FiscalYearId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CostCenter).WithMany()
         .HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.SetNull);
    }
}
