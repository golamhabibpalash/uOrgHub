using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmployeeCode).IsRequired().HasMaxLength(20);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.NationalId).HasMaxLength(30);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.EmergencyContact).HasMaxLength(100);
        builder.Property(x => x.EmergencyPhone).HasMaxLength(20);
        builder.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.EmployeeCode).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
    }
}
