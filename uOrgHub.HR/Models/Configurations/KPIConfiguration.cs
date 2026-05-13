using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class KPIConfiguration : IEntityTypeConfiguration<KPI>
{
    public void Configure(EntityTypeBuilder<KPI> b)
    {
        b.HasKey(x => x.Id);
        b.HasOne(x => x.Department).WithMany()
         .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.Designation).WithMany()
         .HasForeignKey(x => x.DesignationId).OnDelete(DeleteBehavior.SetNull);
    }
}
