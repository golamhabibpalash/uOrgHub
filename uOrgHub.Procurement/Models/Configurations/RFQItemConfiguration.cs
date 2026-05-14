using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class RFQItemConfiguration : IEntityTypeConfiguration<RFQItem>
{
    public void Configure(EntityTypeBuilder<RFQItem> b)
    {
        b.HasKey(x => x.Id);
    }
}
