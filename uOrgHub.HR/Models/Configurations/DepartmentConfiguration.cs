using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasMany(x => x.Designations)
               .WithOne(x => x.Department)
               .HasForeignKey(x => x.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Employees)
               .WithOne(x => x.Department)
               .HasForeignKey(x => x.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
