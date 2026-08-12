using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uOrgHub.Accounts.Models.Entities;

namespace uOrgHub.Accounts.Models.Configurations;

public class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> b)
    {
        b.HasKey(x => x.Id);

        // EntryDate is the list's default sort key and the range bound every dated report filters
        // on, so it carries the ordering cost for the whole module. Status backs the list filter.
        //
        // The unique index on EntryNumber is deliberately not declared here: it was created as raw
        // SQL in AddAccountsModule and so is absent from the model snapshot. Declaring it would
        // make EF emit a CreateIndex for a name that already exists in the database.
        b.HasIndex(x => x.EntryDate);
        b.HasIndex(x => x.Status);
    }
}
