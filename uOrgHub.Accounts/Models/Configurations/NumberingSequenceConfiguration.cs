using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class NumberingSequenceConfiguration : IEntityTypeConfiguration<NumberingSequence>
{
    public void Configure(EntityTypeBuilder<NumberingSequence> b)
    {
        b.HasKey(x => x.Id);
        // CompanyId first: each sister concern counts its own sequence per document
        // type/prefix/period (SISTER_CONCERN_PLAN.md §7) — without it, two companies' first bill
        // of the month would race for the same counter row.
        b.HasIndex(x => new { x.CompanyId, x.DocumentType, x.Prefix, x.Year, x.Month }).IsUnique();
        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
    }
}
