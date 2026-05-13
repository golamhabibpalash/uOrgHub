using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class PayrollCycleConfiguration : IEntityTypeConfiguration<PayrollCycle>
{
    public void Configure(EntityTypeBuilder<PayrollCycle> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.Year, x.Month }).IsUnique();
    }
}
