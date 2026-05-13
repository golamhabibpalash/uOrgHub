using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class EmployeeTrainingConfiguration : IEntityTypeConfiguration<EmployeeTraining>
{
    public void Configure(EntityTypeBuilder<EmployeeTraining> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.EmployeeId, x.TrainingProgramId }).IsUnique();
        b.HasOne(x => x.Employee).WithMany()
         .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TrainingProgram).WithMany(x => x.EmployeeTrainings)
         .HasForeignKey(x => x.TrainingProgramId).OnDelete(DeleteBehavior.Restrict);
    }
}
