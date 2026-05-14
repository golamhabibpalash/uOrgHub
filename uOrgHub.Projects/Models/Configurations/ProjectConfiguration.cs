using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.ProjectCode).IsUnique();

        b.Property(x => x.ContractValue).HasColumnType("decimal(18,2)");

        b.HasOne(x => x.Client)
         .WithMany(x => x.Projects)
         .HasForeignKey(x => x.ClientId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Category)
         .WithMany(x => x.Projects)
         .HasForeignKey(x => x.CategoryId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
