using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> b)
    {
        b.HasKey(x => x.Id);

        // EntryDate is the list's default sort key and the range bound every dated report filters
        // on, so it carries the ordering cost for the whole module. Status backs the list filter.
        b.HasIndex(x => x.EntryDate);
        b.HasIndex(x => x.Status);

        b.HasOne<Company>().WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);

        // Composite (CompanyId, EntryNumber), replacing the old bare-EntryNumber unique index that
        // used to live as raw SQL in AddAccountsModule (outside the EF model, so it never showed up
        // here) — see the UnifySisterConcernAccounts migration, which drops that raw index by name
        // and lets EF declare this one going forward. Two sister concerns number entries
        // independently (SISTER_CONCERN_PLAN.md §7) and can legitimately collide on EntryNumber.
        b.HasIndex(x => new { x.CompanyId, x.EntryNumber }).IsUnique();
    }
}
