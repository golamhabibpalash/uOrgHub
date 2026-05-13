using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();

        b.HasOne(x => x.Department)
         .WithMany(x => x.Designations)
         .HasForeignKey(x => x.DepartmentId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ParentDesignation)
         .WithMany(x => x.ChildDesignations)
         .HasForeignKey(x => x.ParentDesignationId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.SalaryGrade)
         .WithMany(x => x.Designations)
         .HasForeignKey(x => x.SalaryGradeId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
