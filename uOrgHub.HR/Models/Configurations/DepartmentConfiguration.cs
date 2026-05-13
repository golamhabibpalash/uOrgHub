using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.Code).IsRequired().HasMaxLength(20);

        b.HasOne(x => x.ParentDepartment)
         .WithMany(x => x.ChildDepartments)
         .HasForeignKey(x => x.ParentDepartmentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.HeadOfDepartment)
         .WithMany()
         .HasForeignKey(x => x.HeadOfDepartmentId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
