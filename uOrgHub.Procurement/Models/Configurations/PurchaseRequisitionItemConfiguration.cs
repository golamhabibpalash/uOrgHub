using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Procurement.Models.Entities;

namespace uOrgHub.Procurement.Models.Configurations;

public class PurchaseRequisitionItemConfiguration : IEntityTypeConfiguration<PurchaseRequisitionItem>
{
    public void Configure(EntityTypeBuilder<PurchaseRequisitionItem> b)
    {
        b.HasKey(x => x.Id);
    }
}
