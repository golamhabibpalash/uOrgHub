using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class EmployeeSalaryComponentConfiguration : IEntityTypeConfiguration<EmployeeSalaryComponent>
{
    public void Configure(EntityTypeBuilder<EmployeeSalaryComponent> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.EmployeeSalaryStructureId, x.SalaryComponentId }).IsUnique();
        b.HasOne(x => x.SalaryStructure).WithMany(x => x.Components)
         .HasForeignKey(x => x.EmployeeSalaryStructureId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.SalaryComponent).WithMany()
         .HasForeignKey(x => x.SalaryComponentId).OnDelete(DeleteBehavior.Restrict);
    }
}
