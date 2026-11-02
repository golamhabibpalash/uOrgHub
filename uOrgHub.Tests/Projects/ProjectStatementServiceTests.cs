using FluentAssertions;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Projects.DTOs.Reports;
using uOrgHub.Projects.Models.Entities;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Projects.Services;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Tests.Projects;

/// <summary>
/// The project statement is the itemised form of the project financial summary, so it is held to
/// the same rules: posted entries only, attributed by cost center, contra vouchers excluded from
/// cash flow. What is specific here is the date window — figures must move in and out of the
/// period on the entry date, and the opening figure must hold everything before it.
/// </summary>
public class ProjectStatementServiceTests
{
    private static readonly DateTime Jan = new(2026, 1, 15);
    private static readonly DateTime Jun = new(2026, 6, 15);
    private static readonly DateTime Dec = new(2026, 12, 15);

    private static AppDbContext NewContext()
        => TestDb.NewContext("TestDb_ProjectStatement_" + Guid.NewGuid());

    private sealed record Fixture(
        Project Project, CostCenter CostCenter,
        ChartOfAccount Expense, ChartOfAccount Payable, ChartOfAccount Income, ChartOfAccount Bank);

    private static Fixture Seed(
        AppDbContext ctx, bool withCostCenter = true,
        string code = "PRJ-100", string name = "Riverside Bridge", decimal contractValue = 10_000_000m)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectCode = code,
            ProjectName = name,
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            ProjectManagerId = Guid.NewGuid(),
            StartDate = Jan,
            PlannedEndDate = Dec,
            ContractValue = contractValue,
        };
        ctx.Set<Project>().Add(project);

        var costCenter = new CostCenter
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            ProjectId = project.Id,
        };
        if (withCostCenter) ctx.Set<CostCenter>().Add(costCenter);

        var expense = NewAccount("5010", "Construction Materials", AccountGroupType.Expense);
        var payable = NewAccount("2010", "Accounts Payable", AccountGroupType.Liability);
        var income = NewAccount("4010", "Contract Revenue", AccountGroupType.Income);
        var bank = NewAccount("1010", "Cash at Bank", AccountGroupType.Asset);
        ctx.Set<ChartOfAccount>().AddRange(expense, payable, income, bank);

        ctx.SaveChanges();
        return new Fixture(project, costCenter, expense, payable, income, bank);
    }

    private static ChartOfAccount NewAccount(string code, string name, AccountGroupType type)
        => new()
        {
            Id = Guid.NewGuid(),
            AccountCode = code,
            AccountName = name,
            AccountType = type,
            AccountGroupId = Guid.NewGuid(),
        };

    /// <summary>A two-line posting attributed to the project's cost center.</summary>
    private static void Post(
        AppDbContext ctx, Fixture f, ChartOfAccount debit, ChartOfAccount credit,
        decimal amount, DateTime date, JournalEntryStatus status = JournalEntryStatus.Posted)
    {
        var je = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = "JV-" + Guid.NewGuid().ToString()[..8],
            EntryDate = date,
            Status = status,
            TotalDebit = amount,
            TotalCredit = amount,
        };
        je.Lines.Add(new JournalEntryLine
        {
            JournalEntryId = je.Id, AccountId = debit.Id,
            DebitAmount = amount, LineOrder = 1, CostCenterId = f.CostCenter.Id,
        });
        je.Lines.Add(new JournalEntryLine
        {
            JournalEntryId = je.Id, AccountId = credit.Id,
            CreditAmount = amount, LineOrder = 2, CostCenterId = f.CostCenter.Id,
        });
        ctx.Set<JournalEntry>().Add(je);
        ctx.SaveChanges();
    }

    private static void AddVoucher(
        AppDbContext ctx, Fixture f, VoucherType type, decimal amount, DateTime date,
        VoucherStatus status = VoucherStatus.Posted)
    {
        ctx.Set<Voucher>().Add(new Voucher
        {
            Id = Guid.NewGuid(),
            VoucherNumber = "V-" + Guid.NewGuid().ToString()[..6],
            VoucherType = type,
            VoucherDate = date,
            Description = "Cash movement",
            ProjectId = f.Project.Id,
            CostCenterId = f.CostCenter.Id,
            DebitAccountId = f.Expense.Id,
            CreditAccountId = f.Bank.Id,
            Amount = amount,
            Status = status,
        });
        ctx.SaveChanges();
    }

    private static Task<uOrgHub.Projects.DTOs.Reports.ProjectStatementDto> Statement(
        AppDbContext ctx, Fixture f, DateTime? from = null, DateTime? to = null)
        => new ProjectStatementService(ctx).GetStatementAsync(f.Project.Id, from, to);

    private static Task<uOrgHub.Projects.DTOs.Reports.ConsolidatedProjectStatementDto> Consolidated(
        AppDbContext ctx, DateTime? from = null, DateTime? to = null)
        => new ProjectStatementService(ctx).GetConsolidatedStatementAsync(from, to);

    // --- Identity and period framing ---

    [Fact]
    public async Task Returns_the_project_identity_and_contract_value()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);

        var result = await Statement(ctx, f);

        result.ProjectCode.Should().Be("PRJ-100");
        result.ProjectName.Should().Be("Riverside Bridge");
        result.ContractValue.Should().Be(10_000_000m);
    }

    [Fact]
    public async Task Unknown_project_is_not_found()
    {
        using var ctx = NewContext();
        Seed(ctx);

        var act = () => new ProjectStatementService(ctx).GetStatementAsync(Guid.NewGuid(), null, null);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Start_date_after_end_date_is_rejected()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);

        var act = () => Statement(ctx, f, Dec, Jan);

        await act.Should().ThrowAsync<AppException>();
    }

    /// <summary>
    /// A project with no cost center has nothing charged to it yet — an ordinary early state,
    /// not a failure.
    /// </summary>
    [Fact]
    public async Task Project_without_a_cost_center_returns_an_empty_statement()
    {
        using var ctx = NewContext();
        var f = Seed(ctx, withCostCenter: false);

        var result = await Statement(ctx, f);

        result.Rows.Should().BeEmpty();
        result.PeriodExpense.Should().Be(0);
        result.ProjectName.Should().Be("Riverside Bridge");
    }

    // --- Date windowing ---

    [Fact]
    public async Task Only_entries_inside_the_window_count_towards_the_period()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Payable, 100_000, Jan);
        Post(ctx, f, f.Expense, f.Payable, 250_000, Jun);
        Post(ctx, f, f.Expense, f.Payable, 400_000, Dec);

        var result = await Statement(ctx, f, new DateTime(2026, 6, 1), new DateTime(2026, 6, 30));

        result.PeriodExpense.Should().Be(250_000);
        result.Rows.Should().HaveCount(2); // the expense line and its payable counterpart
    }

    [Fact]
    public async Task Opening_spend_holds_everything_before_the_start_date()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Payable, 100_000, Jan);
        Post(ctx, f, f.Expense, f.Payable, 250_000, Jun);

        var result = await Statement(ctx, f, new DateTime(2026, 6, 1), null);

        result.OpeningSpend.Should().Be(100_000);
        result.PeriodExpense.Should().Be(250_000);
        result.ClosingSpend.Should().Be(350_000);
    }

    [Fact]
    public async Task An_open_range_reports_the_whole_project_with_no_opening()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Payable, 100_000, Jan);
        Post(ctx, f, f.Expense, f.Payable, 250_000, Dec);

        var result = await Statement(ctx, f);

        result.OpeningSpend.Should().Be(0);
        result.PeriodExpense.Should().Be(350_000);
    }

    // --- What counts ---

    [Fact]
    public async Task Draft_entries_are_excluded()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Payable, 100_000, Jun);
        Post(ctx, f, f.Expense, f.Payable, 999_999, Jun, JournalEntryStatus.Draft);

        var result = await Statement(ctx, f);

        result.PeriodExpense.Should().Be(100_000);
    }

    [Fact]
    public async Task Another_projects_cost_center_is_excluded()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        var other = Seed(ctx);
        Post(ctx, f, f.Expense, f.Payable, 100_000, Jun);
        Post(ctx, other, other.Expense, other.Payable, 500_000, Jun);

        var result = await Statement(ctx, f);

        result.PeriodExpense.Should().Be(100_000);
    }

    /// <summary>Income is credit-normal, so its sign is the mirror of expense.</summary>
    [Fact]
    public async Task Income_is_reported_credit_normal()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Bank, f.Income, 750_000, Jun);

        var result = await Statement(ctx, f);

        result.PeriodIncome.Should().Be(750_000);
        result.PeriodExpense.Should().Be(0);
    }

    // --- Ledger rows ---

    [Fact]
    public async Task Rows_are_ordered_oldest_first_and_carry_a_running_net()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Payable, 100_000, Jun);
        Post(ctx, f, f.Expense, f.Payable, 50_000, Dec);

        var result = await Statement(ctx, f);

        result.Rows.Should().HaveCount(4);
        result.Rows.First().EntryDate.Should().Be(Jun);
        // Each posting nets to zero across its two lines, so the running net returns to zero.
        result.Rows[0].RunningNet.Should().Be(100_000);
        result.Rows[1].RunningNet.Should().Be(0);
        result.Rows.Last().RunningNet.Should().Be(0);
    }

    [Fact]
    public async Task Rows_carry_the_voucher_party_by_direction()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Bank, 100_000, Jan);
        Post(ctx, f, f.Bank, f.Income, 300_000, Jun);
        Post(ctx, f, f.Expense, f.Payable, 50_000, Dec);
        var entries = ctx.Set<JournalEntry>().OrderBy(e => e.EntryDate).ToList();
        LinkVoucher(ctx, f, VoucherType.Debit, " ABC Traders ", entries[0].Id);
        LinkVoucher(ctx, f, VoucherType.Credit, "City Corporation", entries[1].Id);

        var result = await Statement(ctx, f);

        var paid = result.Rows.Where(r => r.EntryDate == Jan).ToList();
        paid.Should().OnlyContain(r => r.PaidTo == "ABC Traders" && r.ReceivedFrom == null);
        var received = result.Rows.Where(r => r.EntryDate == Jun).ToList();
        received.Should().OnlyContain(r => r.ReceivedFrom == "City Corporation" && r.PaidTo == null);
        // A plain journal with no voucher behind it names no party.
        result.Rows.Where(r => r.EntryDate == Dec)
            .Should().OnlyContain(r => r.PaidTo == null && r.ReceivedFrom == null);
    }

    [Fact]
    public async Task A_corrected_vouchers_reversal_entry_keeps_its_party()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Bank, 100_000, Jan);
        Post(ctx, f, f.Bank, f.Expense, 100_000, Jun);
        var entries = ctx.Set<JournalEntry>().OrderBy(e => e.EntryDate).ToList();
        LinkVoucher(ctx, f, VoucherType.Debit, "ABC Traders", entries[0].Id, reversalEntryId: entries[1].Id);

        var result = await Statement(ctx, f);

        result.Rows.Should().HaveCount(4).And.OnlyContain(r => r.PaidTo == "ABC Traders");
    }

    [Fact]
    public async Task Contra_voucher_name_is_not_treated_as_a_party()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Bank, f.Bank, 10_000, Jan);
        LinkVoucher(ctx, f, VoucherType.Contra, "CHQ-00123", ctx.Set<JournalEntry>().Single().Id);

        var result = await Statement(ctx, f);

        result.Rows.Should().OnlyContain(r => r.PaidTo == null && r.ReceivedFrom == null);
    }

    [Fact]
    public async Task Voucher_lines_are_classified_as_receipts_or_payments()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Bank, 100_000, Jan);
        Post(ctx, f, f.Bank, f.Income, 300_000, Jun);
        Post(ctx, f, f.Expense, f.Payable, 50_000, Dec);
        var entries = ctx.Set<JournalEntry>().OrderBy(e => e.EntryDate).ToList();
        // A blank name must not stop the voucher counting as a payment.
        LinkVoucher(ctx, f, VoucherType.Debit, "", entries[0].Id);
        LinkVoucher(ctx, f, VoucherType.Credit, "City Corporation", entries[1].Id);

        var result = await Statement(ctx, f);

        result.Rows.Where(r => r.EntryDate == Jan).Should().OnlyContain(r => r.CashFlow == ProjectCashFlowType.Payment);
        result.Rows.Where(r => r.EntryDate == Jun).Should().OnlyContain(r => r.CashFlow == ProjectCashFlowType.Receipt);
        result.Rows.Where(r => r.EntryDate == Dec).Should().OnlyContain(r => r.CashFlow == null);
    }

    [Fact]
    public async Task A_voucher_charged_to_another_project_is_not_this_projects_cash_flow()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Bank, 100_000, Jan);
        LinkVoucher(ctx, f, VoucherType.Debit, "ABC Traders", ctx.Set<JournalEntry>().Single().Id, projectId: Guid.NewGuid());

        var result = await Statement(ctx, f);

        result.Rows.Should().OnlyContain(r => r.CashFlow == null);
    }

    [Theory]
    [InlineData(ProjectStatementTransactionFilter.Receipts, ProjectCashFlowType.Receipt)]
    [InlineData(ProjectStatementTransactionFilter.Payments, ProjectCashFlowType.Payment)]
    public async Task Transaction_filter_narrows_only_the_rows(
        ProjectStatementTransactionFilter filter, ProjectCashFlowType expected)
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Bank, 100_000, Jan);
        Post(ctx, f, f.Bank, f.Income, 300_000, Jun);
        Post(ctx, f, f.Expense, f.Payable, 50_000, Dec);
        var entries = ctx.Set<JournalEntry>().OrderBy(e => e.EntryDate).ToList();
        LinkVoucher(ctx, f, VoucherType.Debit, "ABC Traders", entries[0].Id);
        LinkVoucher(ctx, f, VoucherType.Credit, "City Corporation", entries[1].Id);
        AddVoucher(ctx, f, VoucherType.Credit, 500_000, Jun);
        AddVoucher(ctx, f, VoucherType.Debit, 200_000, Jun);

        var all = await Statement(ctx, f);
        var filtered = await new ProjectStatementService(ctx).GetStatementAsync(f.Project.Id, null, null, new ProjectStatementTransactionQuery { TransactionType = filter });

        filtered.Rows.Should().HaveCount(2).And.OnlyContain(r => r.CashFlow == expected);
        // Running net is the full ledger's, not recomputed over the filtered subset.
        filtered.Rows.Should().OnlyContain(r => all.Rows.Any(a => a.EntryNumber == r.EntryNumber && a.RunningNet == r.RunningNet));
        filtered.Should().BeEquivalentTo(all, o => o.Excluding(s => s.Rows));
    }

    [Fact]
    public async Task All_filter_returns_the_unfiltered_statement()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Bank, 100_000, Jan);
        Post(ctx, f, f.Expense, f.Payable, 50_000, Dec);
        LinkVoucher(ctx, f, VoucherType.Debit, "ABC Traders", ctx.Set<JournalEntry>().OrderBy(e => e.EntryDate).First().Id);

        var defaulted = await Statement(ctx, f);
        var all = await new ProjectStatementService(ctx)
            .GetStatementAsync(f.Project.Id, null, null, new ProjectStatementTransactionQuery());

        all.Should().BeEquivalentTo(defaulted);
        all.Rows.Should().HaveCount(4);
    }

    [Fact]
    public async Task Undefined_transaction_filter_is_rejected()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);

        var act = () => new ProjectStatementService(ctx)
            .GetStatementAsync(f.Project.Id, null, null, new ProjectStatementTransactionQuery { TransactionType = (ProjectStatementTransactionFilter)7 });

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Undefined_side_filter_is_rejected()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);

        var act = () => Filtered(ctx, f, new ProjectStatementTransactionQuery { Side = (ProjectStatementLineSide)9 });

        await act.Should().ThrowAsync<AppException>();
    }

    /// <summary>
    /// Jan: payment to ABC Traders (expense / bank). Jun: receipt from City Corporation
    /// (bank / income). Dec: a plain journal (expense / payable). Six ledger lines.
    /// </summary>
    private static Fixture SeedPartyLedger(AppDbContext ctx)
    {
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Bank, 100_000, Jan);
        Post(ctx, f, f.Bank, f.Income, 300_000, Jun);
        Post(ctx, f, f.Expense, f.Payable, 50_000, Dec);
        var entries = ctx.Set<JournalEntry>().OrderBy(e => e.EntryDate).ToList();
        LinkVoucher(ctx, f, VoucherType.Debit, "ABC Traders", entries[0].Id);
        LinkVoucher(ctx, f, VoucherType.Credit, "City Corporation", entries[1].Id);
        return f;
    }

    private static Task<ProjectStatementDto> Filtered(AppDbContext ctx, Fixture f, ProjectStatementTransactionQuery q)
        => new ProjectStatementService(ctx).GetStatementAsync(f.Project.Id, null, null, q);

    [Fact]
    public async Task Search_matches_any_listed_field_ignoring_case()
    {
        using var ctx = NewContext();
        var f = SeedPartyLedger(ctx);

        (await Filtered(ctx, f, new() { Search = "  construction MATERIALS " })).Rows
            .Should().HaveCount(2).And.OnlyContain(r => r.AccountId == f.Expense.Id);
        (await Filtered(ctx, f, new() { Search = "abc trad" })).Rows
            .Should().HaveCount(2).And.OnlyContain(r => r.PaidTo == "ABC Traders");
    }

    [Fact]
    public async Task Account_and_side_filters_narrow_the_rows()
    {
        using var ctx = NewContext();
        var f = SeedPartyLedger(ctx);

        (await Filtered(ctx, f, new() { AccountId = f.Bank.Id })).Rows
            .Should().HaveCount(2).And.OnlyContain(r => r.AccountId == f.Bank.Id);
        (await Filtered(ctx, f, new() { Side = ProjectStatementLineSide.Debit })).Rows
            .Should().HaveCount(3).And.OnlyContain(r => r.Debit > 0);
        (await Filtered(ctx, f, new() { Side = ProjectStatementLineSide.Credit })).Rows
            .Should().HaveCount(3).And.OnlyContain(r => r.Credit > 0);
    }

    [Fact]
    public async Task Party_filters_match_the_name_ignoring_case_and_spacing()
    {
        using var ctx = NewContext();
        var f = SeedPartyLedger(ctx);

        (await Filtered(ctx, f, new() { PaidTo = " abc traders " })).Rows
            .Should().HaveCount(2).And.OnlyContain(r => r.PaidTo == "ABC Traders");
        (await Filtered(ctx, f, new() { ReceivedFrom = "CITY CORPORATION" })).Rows
            .Should().HaveCount(2).And.OnlyContain(r => r.ReceivedFrom == "City Corporation");
    }

    [Fact]
    public async Task Filters_combine_and_never_change_the_summary()
    {
        using var ctx = NewContext();
        var f = SeedPartyLedger(ctx);

        var all = await Statement(ctx, f);
        var filtered = await Filtered(ctx, f, new()
        {
            TransactionType = ProjectStatementTransactionFilter.Payments,
            Side = ProjectStatementLineSide.Credit,
            PaidTo = "ABC Traders",
        });

        var row = filtered.Rows.Should().ContainSingle().Subject;
        row.AccountId.Should().Be(f.Bank.Id);
        row.RunningNet.Should().Be(all.Rows.Single(r => r.EntryNumber == row.EntryNumber && r.Credit > 0).RunningNet);
        filtered.Should().BeEquivalentTo(all, o => o.Excluding(s => s.Rows));
    }

    [Fact]
    public async Task Sort_orders_the_rows_and_keeps_posting_order_on_ties()
    {
        using var ctx = NewContext();
        var f = SeedPartyLedger(ctx);

        var all = await Statement(ctx, f);
        var byDebit = await Filtered(ctx, f, new() { SortBy = "debit", SortDescending = true });

        byDebit.Rows.Select(r => r.Debit).Should().BeInDescendingOrder();
        // The three zero-debit lines tie; they must keep the order the ledger posted them in.
        byDebit.Rows.Where(r => r.Debit == 0).Select(r => r.EntryNumber + r.AccountCode)
            .Should().Equal(all.Rows.Where(r => r.Debit == 0).Select(r => r.EntryNumber + r.AccountCode));
    }

    [Fact]
    public async Task Unknown_sort_column_keeps_posting_order()
    {
        using var ctx = NewContext();
        var f = SeedPartyLedger(ctx);

        var all = await Statement(ctx, f);
        var unknown = await Filtered(ctx, f, new() { SortBy = "noSuchColumn" });

        unknown.Rows.Should().Equal(all.Rows);
    }

    /// <summary>A posted voucher naming a party, tied to an already-posted entry.</summary>
    private static void LinkVoucher(
        AppDbContext ctx, Fixture f, VoucherType type, string name, Guid entryId,
        Guid? reversalEntryId = null, Guid? projectId = null)
    {
        ctx.Set<Voucher>().Add(new Voucher
        {
            Id = Guid.NewGuid(),
            VoucherNumber = "V-" + Guid.NewGuid().ToString()[..6],
            VoucherType = type,
            VoucherDate = Jan,
            Name = name,
            Description = "Party voucher",
            ProjectId = projectId ?? f.Project.Id,
            CostCenterId = f.CostCenter.Id,
            DebitAccountId = f.Expense.Id,
            CreditAccountId = f.Bank.Id,
            Amount = 1,
            Status = VoucherStatus.Posted,
            JournalEntryId = entryId,
            ReversalJournalEntryId = reversalEntryId,
        });
        ctx.SaveChanges();
    }

    [Fact]
    public async Task Account_breakdown_groups_period_totals_per_account()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        Post(ctx, f, f.Expense, f.Payable, 100_000, Jun);
        Post(ctx, f, f.Expense, f.Payable, 60_000, Dec);

        var result = await Statement(ctx, f);

        var expenseRow = result.ByAccount.Single(a => a.AccountCode == "5010");
        expenseRow.Debit.Should().Be(160_000);
        expenseRow.Credit.Should().Be(0);
        expenseRow.Net.Should().Be(160_000);

        var payableRow = result.ByAccount.Single(a => a.AccountCode == "2010");
        payableRow.Credit.Should().Be(160_000);
        payableRow.Net.Should().Be(-160_000);
    }

    // --- Voucher cash flow ---

    [Fact]
    public async Task Receipts_and_payments_come_from_posted_vouchers_in_the_period()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        AddVoucher(ctx, f, VoucherType.Credit, 500_000, Jun);
        AddVoucher(ctx, f, VoucherType.Debit, 200_000, Jun);
        AddVoucher(ctx, f, VoucherType.Debit, 900_000, Dec); // outside the window

        var result = await Statement(ctx, f, new DateTime(2026, 6, 1), new DateTime(2026, 6, 30));

        result.Receipts.Should().Be(500_000);
        result.Payments.Should().Be(200_000);
        result.NetCashPosition.Should().Be(300_000);
    }

    /// <summary>
    /// A contra voucher moves money between the organisation's own accounts, so counting it would
    /// inflate the project's cash flow with money that never entered or left it.
    /// </summary>
    [Fact]
    public async Task Contra_vouchers_are_excluded_from_cash_flow()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        AddVoucher(ctx, f, VoucherType.Contra, 400_000, Jun);

        var result = await Statement(ctx, f);

        result.Receipts.Should().Be(0);
        result.Payments.Should().Be(0);
    }

    [Fact]
    public async Task Unposted_vouchers_are_excluded_from_cash_flow()
    {
        using var ctx = NewContext();
        var f = Seed(ctx);
        AddVoucher(ctx, f, VoucherType.Credit, 400_000, Jun, VoucherStatus.Submitted);

        var result = await Statement(ctx, f);

        result.Receipts.Should().Be(0);
    }

    // --- Consolidated (all-projects) statement ---

    [Fact]
    public async Task Consolidated_statement_rejects_a_start_after_the_end()
    {
        using var ctx = NewContext();
        Seed(ctx);

        var act = () => Consolidated(ctx, Dec, Jan);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Consolidated_totals_are_the_sum_of_every_projects_figures()
    {
        using var ctx = NewContext();
        var a = Seed(ctx, code: "PRJ-A", name: "Bridge", contractValue: 6_000_000m);
        var b = Seed(ctx, code: "PRJ-B", name: "Road", contractValue: 4_000_000m);
        Post(ctx, a, a.Expense, a.Payable, 100_000, Jun);
        Post(ctx, b, b.Expense, b.Payable, 250_000, Jun);
        AddVoucher(ctx, a, VoucherType.Credit, 500_000, Jun);
        AddVoucher(ctx, b, VoucherType.Debit, 120_000, Jun);

        var result = await Consolidated(ctx);

        result.Projects.Should().HaveCount(2);
        result.ContractValue.Should().Be(10_000_000m);
        result.PeriodExpense.Should().Be(350_000);
        result.Receipts.Should().Be(500_000);
        result.Payments.Should().Be(120_000);
        result.NetCashPosition.Should().Be(380_000);
    }

    [Fact]
    public async Task Consolidated_rows_are_ordered_by_project_code()
    {
        using var ctx = NewContext();
        Seed(ctx, code: "PRJ-Z", name: "Zeta");
        Seed(ctx, code: "PRJ-A", name: "Alpha");

        var result = await Consolidated(ctx);

        result.Projects.Select(p => p.ProjectCode).Should().ContainInOrder("PRJ-A", "PRJ-Z");
    }

    [Fact]
    public async Task Consolidated_statement_lists_projects_with_no_activity()
    {
        using var ctx = NewContext();
        Seed(ctx, code: "PRJ-A", name: "Idle", contractValue: 2_000_000m);

        var result = await Consolidated(ctx);

        var row = result.Projects.Single();
        row.ContractValue.Should().Be(2_000_000m);
        row.PeriodExpense.Should().Be(0);
        row.NetCashPosition.Should().Be(0);
    }

    [Fact]
    public async Task Consolidated_attributes_each_posting_to_its_own_project()
    {
        using var ctx = NewContext();
        var a = Seed(ctx, code: "PRJ-A", name: "Bridge");
        var b = Seed(ctx, code: "PRJ-B", name: "Road");
        Post(ctx, a, a.Expense, a.Payable, 100_000, Jun);
        Post(ctx, b, b.Expense, b.Payable, 250_000, Jun);

        var result = await Consolidated(ctx);

        result.Projects.Single(p => p.ProjectCode == "PRJ-A").PeriodExpense.Should().Be(100_000);
        result.Projects.Single(p => p.ProjectCode == "PRJ-B").PeriodExpense.Should().Be(250_000);
    }

    [Fact]
    public async Task Consolidated_excludes_draft_entries_and_contra_vouchers()
    {
        using var ctx = NewContext();
        var f = Seed(ctx, code: "PRJ-A");
        Post(ctx, f, f.Expense, f.Payable, 100_000, Jun);
        Post(ctx, f, f.Expense, f.Payable, 999_999, Jun, JournalEntryStatus.Draft);
        AddVoucher(ctx, f, VoucherType.Contra, 400_000, Jun);

        var result = await Consolidated(ctx);

        result.PeriodExpense.Should().Be(100_000);
        result.Receipts.Should().Be(0);
        result.Payments.Should().Be(0);
    }

    [Fact]
    public async Task Consolidated_opening_spend_holds_everything_before_the_start_date()
    {
        using var ctx = NewContext();
        var f = Seed(ctx, code: "PRJ-A");
        Post(ctx, f, f.Expense, f.Payable, 100_000, Jan);
        Post(ctx, f, f.Expense, f.Payable, 250_000, Jun);

        var result = await Consolidated(ctx, new DateTime(2026, 6, 1), null);

        result.OpeningSpend.Should().Be(100_000);
        result.PeriodExpense.Should().Be(250_000);
        result.ClosingSpend.Should().Be(350_000);
    }

    /// <summary>
    /// The strong invariant: a project's row in the consolidated statement must equal the figures
    /// its own statement reports for the same window.
    /// </summary>
    [Fact]
    public async Task Consolidated_row_reconciles_to_the_single_project_statement()
    {
        using var ctx = NewContext();
        var a = Seed(ctx, code: "PRJ-A", name: "Bridge");
        var b = Seed(ctx, code: "PRJ-B", name: "Road");
        Post(ctx, a, a.Expense, a.Payable, 100_000, Jan);
        Post(ctx, a, a.Expense, a.Payable, 250_000, Jun);
        Post(ctx, a, a.Bank, a.Income, 400_000, Jun);
        Post(ctx, b, b.Expense, b.Payable, 999_000, Jun);
        AddVoucher(ctx, a, VoucherType.Credit, 500_000, Jun);
        AddVoucher(ctx, a, VoucherType.Debit, 200_000, Jun);

        var from = new DateTime(2026, 6, 1);
        var to = new DateTime(2026, 6, 30);
        var single = await Statement(ctx, a, from, to);
        var row = (await Consolidated(ctx, from, to)).Projects.Single(p => p.ProjectCode == "PRJ-A");

        row.ContractValue.Should().Be(single.ContractValue);
        row.OpeningSpend.Should().Be(single.OpeningSpend);
        row.PeriodExpense.Should().Be(single.PeriodExpense);
        row.PeriodIncome.Should().Be(single.PeriodIncome);
        row.ClosingSpend.Should().Be(single.ClosingSpend);
        row.Receipts.Should().Be(single.Receipts);
        row.Payments.Should().Be(single.Payments);
        row.NetCashPosition.Should().Be(single.NetCashPosition);
    }
}
