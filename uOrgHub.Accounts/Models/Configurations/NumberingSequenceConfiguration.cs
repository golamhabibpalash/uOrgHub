using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class NumberingSequenceConfiguration : IEntityTypeConfiguration<NumberingSequence>
{
    public void Configure(EntityTypeBuilder<NumberingSequence> b)
    {
        b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.DocumentType, x.Prefix, x.Year, x.Month }).IsUnique();
    }
}
