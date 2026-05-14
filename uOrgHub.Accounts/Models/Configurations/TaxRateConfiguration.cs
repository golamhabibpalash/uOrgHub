using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
{
    public void Configure(EntityTypeBuilder<TaxRate> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.Code).IsUnique();
        b.HasOne(x => x.TaxAccount).WithMany()
         .HasForeignKey(x => x.TaxAccountId).OnDelete(DeleteBehavior.SetNull);
    }
}
