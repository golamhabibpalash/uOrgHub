using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.EmployeeCode).IsUnique();
        b.HasIndex(x => x.Email).IsUnique();

        b.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Designation)
         .WithMany(x => x.Employees)
         .HasForeignKey(x => x.DesignationId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Department)
         .WithMany(x => x.Employees)
         .HasForeignKey(x => x.DepartmentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Manager)
         .WithMany(x => x.DirectReports)
         .HasForeignKey(x => x.ManagerId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.SalaryGrade)
         .WithMany(x => x.Employees)
         .HasForeignKey(x => x.SalaryGradeId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
