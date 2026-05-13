using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasMany(x => x.Employees)
               .WithOne(x => x.Designation)
               .HasForeignKey(x => x.DesignationId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
