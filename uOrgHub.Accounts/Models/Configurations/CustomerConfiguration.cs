using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.CustomerCode).IsUnique();
        b.HasOne(x => x.ReceivableAccount).WithMany()
         .HasForeignKey(x => x.ReceivableAccountId).OnDelete(DeleteBehavior.Restrict);
    }
}
