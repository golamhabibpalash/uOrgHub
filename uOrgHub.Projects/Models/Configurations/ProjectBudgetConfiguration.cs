using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class ProjectBudgetConfiguration : IEntityTypeConfiguration<ProjectBudget>
{
    public void Configure(EntityTypeBuilder<ProjectBudget> b)
    {
        b.HasKey(x => x.Id);

        b.Property(x => x.AllocatedAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.SpentAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.RevisedAmount).HasColumnType("decimal(18,2)");

        b.HasIndex(x => new { x.ProjectId, x.BudgetType, x.FiscalYearId }).IsUnique();

        b.HasOne(x => x.Project)
         .WithMany(x => x.Budgets)
         .HasForeignKey(x => x.ProjectId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
