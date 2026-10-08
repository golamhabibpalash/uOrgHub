using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class PayrollEntryLineConfiguration : IEntityTypeConfiguration<PayrollEntryLine>
{
    public void Configure(EntityTypeBuilder<PayrollEntryLine> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.PayrollEntryId);
        b.HasOne(x => x.PayrollEntry).WithMany(x => x.Lines)
         .HasForeignKey(x => x.PayrollEntryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<SalaryComponent>().WithMany()
         .HasForeignKey(x => x.SalaryComponentId).OnDelete(DeleteBehavior.Restrict);
    }
}
