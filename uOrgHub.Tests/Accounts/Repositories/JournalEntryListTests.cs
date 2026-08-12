using FluentAssertions;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Extensions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Repositories;

/// <summary>
/// The journal entry list is the page that grows without bound, so paging, sorting and search all
/// have to happen in the database rather than after the fact. These cover the contract the grid
/// depends on: a stable order across pages, a case-insensitive search, and a status filter.
/// </summary>
public class JournalEntryListTests
{
    private static AppDbContext NewContext()
        => TestDb.NewContext("TestDb_JournalEntryList_" + Guid.NewGuid());

    private static void Seed(AppDbContext ctx, string entryNumber, DateTime date,
        string description = "Entry", string? reference = null,
        JournalEntryStatus status = JournalEntryStatus.Draft)
    {
        ctx.Set<JournalEntry>().Add(new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = entryNumber,
            EntryDate = date,
            Description = description,
            ReferenceNumber = reference,
            Status = status,
            TotalDebit = 100,
            TotalCredit = 100,
        });
        ctx.SaveChanges();
    }

    private static Task<PagedResult<JournalEntry>> List(AppDbContext ctx, PaginationRequest request)
        => new JournalEntryRepository(ctx).GetAllAsync(request);

    // --- Ordering ---

    [Fact]
    public async Task Defaults_to_newest_first()
    {
        using var ctx = NewContext();
        Seed(ctx, "JV-0001", new DateTime(2026, 1, 10));
        Seed(ctx, "JV-0002", new DateTime(2026, 6, 10));
        Seed(ctx, "JV-0003", new DateTime(2026, 3, 10));

        var result = await List(ctx, new PaginationRequest());

        result.Items.Select(x => x.EntryNumber)
            .Should().ContainInOrder("JV-0002", "JV-0003", "JV-0001");
    }

    /// <summary>
    /// The tie-break must be attached as a ThenBy. Chaining a second OrderBy instead *replaces*
    /// the first, and the tie-break vanishes from the generated SQL — leaving paging free to
    /// repeat or drop rows whenever entries share a date.
    ///
    /// Asserted on the expression tree rather than on results, because the in-memory provider
    /// sorts stably and so returns the right answer either way: a behavioural test here passes
    /// against the broken composition and proves nothing.
    /// </summary>
    [Fact]
    public void Tie_break_is_attached_as_a_then_by()
    {
        var query = new List<JournalEntry>().AsQueryable();

        var sorted = query.ApplySorting("EntryDate", true, x => x.EntryNumber, tieBreakDescending: true);

        sorted.Expression.ToString().Should().Contain("ThenByDescending");
    }

    /// <summary>With no usable primary sort the tie-break alone still totally orders the query.</summary>
    [Fact]
    public void Unknown_sort_field_falls_back_to_the_tie_break()
    {
        var query = new List<JournalEntry>().AsQueryable();

        var sorted = query.ApplySorting("NoSuchColumn", true, x => x.EntryNumber, tieBreakDescending: true);

        sorted.Expression.ToString().Should().Contain("OrderByDescending");
    }

    /// <summary>
    /// Entries posted on the same day are common. Without a unique tie-break the database may
    /// order them differently per page, which silently repeats or drops rows as the user pages.
    /// </summary>
    [Fact]
    public async Task Entries_sharing_a_date_are_ordered_deterministically()
    {
        using var ctx = NewContext();
        var sameDay = new DateTime(2026, 5, 1);
        Seed(ctx, "JV-0001", sameDay);
        Seed(ctx, "JV-0003", sameDay);
        Seed(ctx, "JV-0002", sameDay);

        var page1 = await List(ctx, new PaginationRequest { Page = 1, PageSize = 2 });
        var page2 = await List(ctx, new PaginationRequest { Page = 2, PageSize = 2 });

        var seen = page1.Items.Concat(page2.Items).Select(x => x.EntryNumber).ToList();
        seen.Should().OnlyHaveUniqueItems();
        seen.Should().ContainInOrder("JV-0003", "JV-0002", "JV-0001");
    }

    [Fact]
    public async Task Explicit_sort_overrides_the_default()
    {
        using var ctx = NewContext();
        Seed(ctx, "JV-0001", new DateTime(2026, 1, 10));
        Seed(ctx, "JV-0002", new DateTime(2026, 6, 10));

        var result = await List(ctx, new PaginationRequest { SortBy = "entryDate", SortDescending = false });

        result.Items.First().EntryNumber.Should().Be("JV-0001");
    }

    /// <summary>The grid sends camelCase column keys; the sort must resolve them.</summary>
    [Fact]
    public async Task Sort_field_is_matched_case_insensitively()
    {
        using var ctx = NewContext();
        Seed(ctx, "JV-0002", new DateTime(2026, 1, 1));
        Seed(ctx, "JV-0001", new DateTime(2026, 1, 2));

        var result = await List(ctx, new PaginationRequest { SortBy = "entryNumber", SortDescending = false });

        result.Items.First().EntryNumber.Should().Be("JV-0001");
    }

    // --- Paging ---

    [Fact]
    public async Task Total_count_reflects_the_filtered_set_not_the_page()
    {
        using var ctx = NewContext();
        for (var i = 1; i <= 7; i++)
            Seed(ctx, $"JV-{i:D4}", new DateTime(2026, 1, i));

        var result = await List(ctx, new PaginationRequest { Page = 1, PageSize = 3 });

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(7);
    }

    // --- Search ---

    /// <summary>
    /// PostgreSQL's LIKE is case-sensitive, so an unlowered search would miss the very records the
    /// user is looking at — entry numbers are stored upper case.
    /// </summary>
    [Theory]
    [InlineData("jv-2026")]
    [InlineData("JV-2026")]
    [InlineData("Jv-2026")]
    public async Task Search_matches_regardless_of_case(string term)
    {
        using var ctx = NewContext();
        Seed(ctx, "JV-2026-0001", new DateTime(2026, 1, 1));
        Seed(ctx, "XX-2025-0001", new DateTime(2026, 1, 2));

        var result = await List(ctx, new PaginationRequest { Search = term });

        result.TotalCount.Should().Be(1);
        result.Items.Single().EntryNumber.Should().Be("JV-2026-0001");
    }

    [Fact]
    public async Task Search_covers_description_and_reference()
    {
        using var ctx = NewContext();
        Seed(ctx, "JV-0001", new DateTime(2026, 1, 1), description: "Cement purchase");
        Seed(ctx, "JV-0002", new DateTime(2026, 1, 2), reference: "INV-77");
        Seed(ctx, "JV-0003", new DateTime(2026, 1, 3), description: "Fuel");

        (await List(ctx, new PaginationRequest { Search = "cement" })).TotalCount.Should().Be(1);
        (await List(ctx, new PaginationRequest { Search = "inv-77" })).TotalCount.Should().Be(1);
    }

    /// <summary>A null ReferenceNumber must be a non-match, not a crash.</summary>
    [Fact]
    public async Task Search_tolerates_null_columns()
    {
        using var ctx = NewContext();
        Seed(ctx, "JV-0001", new DateTime(2026, 1, 1), reference: null);

        var act = () => List(ctx, new PaginationRequest { Search = "anything" });

        await act.Should().NotThrowAsync();
        (await List(ctx, new PaginationRequest { Search = "anything" })).TotalCount.Should().Be(0);
    }

    // --- Status filter ---

    [Fact]
    public async Task Status_filter_narrows_to_that_status()
    {
        using var ctx = NewContext();
        Seed(ctx, "JV-0001", new DateTime(2026, 1, 1), status: JournalEntryStatus.Draft);
        Seed(ctx, "JV-0002", new DateTime(2026, 1, 2), status: JournalEntryStatus.Posted);
        Seed(ctx, "JV-0003", new DateTime(2026, 1, 3), status: JournalEntryStatus.Posted);

        var request = new PaginationRequest { Filters = new() { ["Status"] = "Posted" } };
        var result = await List(ctx, request);

        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(x => x.Status == JournalEntryStatus.Posted);
    }

    /// <summary>
    /// A filter value that is not a member of the enum must be ignored rather than reaching
    /// Expression.Constant, where the type mismatch would surface to the user as a 500.
    /// </summary>
    [Fact]
    public async Task Unparseable_status_filter_is_ignored()
    {
        using var ctx = NewContext();
        Seed(ctx, "JV-0001", new DateTime(2026, 1, 1));

        var request = new PaginationRequest { Filters = new() { ["Status"] = "NotAStatus" } };

        var result = await List(ctx, request);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Deleted_entries_are_excluded()
    {
        using var ctx = NewContext();
        Seed(ctx, "JV-0001", new DateTime(2026, 1, 1));
        var gone = ctx.Set<JournalEntry>().First();
        gone.IsDeleted = true;
        ctx.SaveChanges();

        (await List(ctx, new PaginationRequest())).TotalCount.Should().Be(0);
    }
}
