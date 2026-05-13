using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Models.Configurations;

public class OvertimeRuleConfiguration : IEntityTypeConfiguration<OvertimeRule>
{
    public void Configure(EntityTypeBuilder<OvertimeRule> b)
    {
        b.HasKey(x => x.Id);
    }
}
