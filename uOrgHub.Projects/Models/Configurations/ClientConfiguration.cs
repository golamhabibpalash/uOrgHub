using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Projects.Models.Entities;

namespace uOrgHub.Projects.Models.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.ClientCode).IsUnique();
        b.HasOne(x => x.Customer).WithMany()
         .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
    }
}
