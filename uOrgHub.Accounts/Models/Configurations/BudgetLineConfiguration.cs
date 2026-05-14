using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class BudgetLineConfiguration : IEntityTypeConfiguration<BudgetLine>
{
    public void Configure(EntityTypeBuilder<BudgetLine> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Budget).WithMany(x => x.Lines)
         .HasForeignKey(x => x.BudgetId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Account).WithMany()
         .HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CostCenter).WithMany(x => x.BudgetLines)
         .HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.SetNull);
    }
}
