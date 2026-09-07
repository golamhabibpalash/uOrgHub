using FluentAssertions;
using uOrgHub.Accounts.DTOs.Reports;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Data;

namespace uOrgHub.Tests.Accounts;

/// <summary>
/// The Receipts &amp; Payments Statement classifies every journal entry that touches an account
/// flagged <see cref="ChartOfAccount.IsCashOrBank"/> as a receipt (net cash in), a payment
/// (net cash out) or a transfer between own accounts (both legs cash/bank), then rolls the
/// non-cash counterpart accounts up by cost center.
/// </summary>
public class ReceiptsPaymentsReportTests
{
    private static AppDbContext NewContext()
        => TestDb.NewContext("TestDb_ReceiptsPayments_" + Guid.NewGuid());

    private static readonly Guid CashId = Guid.NewGuid();
    private static readonly Guid BankId = Guid.NewGuid();
    private static readonly Guid SalesId = Guid.NewGuid();
    private static readonly Guid ConveyanceId = Guid.NewGuid();
    private static readonly Guid SiteAId = Guid.NewGuid();

    private static void SeedAccounts(AppDbContext ctx)
    {
        ctx.Set<ChartOfAccount>().AddRange(
            new ChartOfAccount { Id = CashId, AccountCode = "1000", AccountName = "Cash", AccountGroupId = Guid.NewGuid(), AccountType = AccountGroupType.Asset, IsCashOrBank = true, OpeningBalance = 1000m },
            new ChartOfAccount { Id = BankId, AccountCode = "1100", AccountName = "Bank", AccountGroupId = Guid.NewGuid(), AccountType = AccountGroupType.Asset, IsCashOrBank = true, OpeningBalance = 5000m },
            new ChartOfAccount { Id = SalesId, AccountCode = "4000", AccountName = "Sales", AccountGroupId = Guid.NewGuid(), AccountType = AccountGroupType.Income },
            new ChartOfAccount { Id = ConveyanceId, AccountCode = "5000", AccountName = "Conveyance", AccountGroupId = Guid.NewGuid(), AccountType = AccountGroupType.Expense });
        ctx.Set<CostCenter>().Add(new CostCenter { Id = SiteAId, Code = "CC1", Name = "Site A" });
        ctx.SaveChanges();
    }

    private static void SeedEntry(
        AppDbContext ctx, string number, DateTime date, JournalEntryStatus status,
        (Guid accountId, decimal debit, decimal credit, Guid? costCenterId)[] lines)
    {
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = number,
            EntryDate = date,
            Description = number,
            Status = status,
            TotalDebit = lines.Sum(l => l.debit),
            TotalCredit = lines.Sum(l => l.credit),
        };
        ctx.Set<JournalEntry>().Add(entry);
        var order = 0;
        foreach (var (accountId, debit, credit, cc) in lines)
        {
            ctx.Set<JournalEntryLine>().Add(new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                JournalEntryId = entry.Id,
                AccountId = accountId,
                DebitAmount = debit,
                CreditAmount = credit,
                CostCenterId = cc,
                LineOrder = order++,
            });
        }
        ctx.SaveChanges();
    }

    /// <summary>Seeds a journal entry with two lines plus a voucher of <paramref name="type"/> linked to it.</summary>
    private static void SeedVoucherEntry(
        AppDbContext ctx, string number, DateTime date, VoucherType type,
        Guid debitAccountId, Guid creditAccountId, decimal amount, Guid? costCenterId, Guid? projectId = null)
    {
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = number,
            EntryDate = date,
            Description = number,
            Status = JournalEntryStatus.Posted,
            TotalDebit = amount,
            TotalCredit = amount,
        };
        ctx.Set<JournalEntry>().Add(entry);
        ctx.Set<JournalEntryLine>().AddRange(
            new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = entry.Id, AccountId = debitAccountId, DebitAmount = amount, CostCenterId = costCenterId, LineOrder = 0 },
            new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = entry.Id, AccountId = creditAccountId, CreditAmount = amount, CostCenterId = costCenterId, LineOrder = 1 });
        ctx.Set<Voucher>().Add(new Voucher
        {
            Id = Guid.NewGuid(),
            VoucherNumber = number,
            VoucherType = type,
            VoucherDate = date,
            Description = number,
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId,
            Amount = amount,
            CostCenterId = costCenterId,
            ProjectId = projectId,
            Status = VoucherStatus.Posted,
            JournalEntryId = entry.Id,
        });
        ctx.SaveChanges();
    }

    private static ReceiptsPaymentsFilterDto August2026 => new(
        new DateTime(2026, 8, 1), new DateTime(2026, 8, 31), null, null, null);

    [Fact]
    public async Task Voucher_type_drives_classification()
    {
        using var ctx = NewContext();
        SeedAccounts(ctx);
        // A Credit voucher is a receipt regardless of which account is flagged.
        SeedVoucherEntry(ctx, "CR-1", new DateTime(2026, 8, 5), VoucherType.Credit, CashId, SalesId, 2000m, SiteAId);
        // A Debit voucher is a payment.
        SeedVoucherEntry(ctx, "DR-1", new DateTime(2026, 8, 6), VoucherType.Debit, ConveyanceId, CashId, 500m, SiteAId);
        // A Contra voucher is a transfer between own accounts.
        SeedVoucherEntry(ctx, "CN-1", new DateTime(2026, 8, 7), VoucherType.Contra, BankId, CashId, 1000m, null);

        var report = await new AccountingReportService(ctx).GetReceiptsPaymentsAsync(August2026);

        report.Receipts.Should().ContainSingle();
        report.Receipts[0].CostCenterName.Should().Be("Site A");
        report.Receipts[0].Rows.Should().ContainSingle(r => r.AccountName == "Sales" && r.Amount == 2000m);
        report.TotalReceiptsExclTransfers.Should().Be(2000m);

        report.Payments.Should().ContainSingle();
        report.Payments[0].Rows.Should().ContainSingle(r => r.AccountName == "Conveyance" && r.Amount == 500m);
        report.TotalPayments.Should().Be(500m);

        report.Transfers.Should().ContainSingle(t => t.FromAccount == "Cash" && t.ToAccount == "Bank" && t.Amount == 1000m);
        report.TotalTransfers.Should().Be(1000m);
    }

    [Fact]
    public async Task Voucher_project_filter_scopes_receipts_and_payments()
    {
        using var ctx = NewContext();
        SeedAccounts(ctx);
        var projectX = Guid.NewGuid();
        SeedVoucherEntry(ctx, "CR-1", new DateTime(2026, 8, 5), VoucherType.Credit, CashId, SalesId, 2000m, SiteAId, projectX);
        SeedVoucherEntry(ctx, "CR-2", new DateTime(2026, 8, 6), VoucherType.Credit, CashId, SalesId, 700m, SiteAId, Guid.NewGuid());

        var filtered = await new AccountingReportService(ctx).GetReceiptsPaymentsAsync(
            new ReceiptsPaymentsFilterDto(new DateTime(2026, 8, 1), new DateTime(2026, 8, 31), null, null, projectX));

        filtered.TotalReceiptsExclTransfers.Should().Be(2000m);
    }

    [Fact]
    public async Task Splits_receipts_payments_and_transfers()
    {
        using var ctx = NewContext();
        SeedAccounts(ctx);
        // Receipt: cash in from a sale, charged to Site A.
        SeedEntry(ctx, "JV-1", new DateTime(2026, 8, 5), JournalEntryStatus.Posted, new[]
        {
            (CashId, 2000m, 0m, (Guid?)null),
            (SalesId, 0m, 2000m, (Guid?)SiteAId),
        });
        // Payment: conveyance paid in cash, charged to Site A.
        SeedEntry(ctx, "JV-2", new DateTime(2026, 8, 6), JournalEntryStatus.Posted, new[]
        {
            (ConveyanceId, 500m, 0m, (Guid?)SiteAId),
            (CashId, 0m, 500m, (Guid?)null),
        });
        // Transfer: cash deposited into the bank — both legs are cash/bank accounts.
        SeedEntry(ctx, "JV-3", new DateTime(2026, 8, 7), JournalEntryStatus.Draft, new[]
        {
            (BankId, 1000m, 0m, (Guid?)null),
            (CashId, 0m, 1000m, (Guid?)null),
        });

        var report = await new AccountingReportService(ctx).GetReceiptsPaymentsAsync(August2026);

        report.Receipts.Should().ContainSingle();
        report.Receipts[0].CostCenterName.Should().Be("Site A");
        report.Receipts[0].Rows.Should().ContainSingle(r => r.AccountName == "Sales" && r.Amount == 2000m);
        report.TotalReceiptsExclTransfers.Should().Be(2000m);

        report.Transfers.Should().ContainSingle();
        report.Transfers[0].FromAccount.Should().Be("Cash");
        report.Transfers[0].ToAccount.Should().Be("Bank");
        report.Transfers[0].Amount.Should().Be(1000m);
        report.TotalTransfers.Should().Be(1000m);
        report.TotalReceiptsInclTransfers.Should().Be(3000m);

        report.Payments.Should().ContainSingle();
        report.Payments[0].Rows.Should().ContainSingle(r => r.AccountName == "Conveyance" && r.Amount == 500m);
        report.TotalPayments.Should().Be(500m);
    }

    [Fact]
    public async Task Bottom_balances_carry_opening_and_close_on_period_movement()
    {
        using var ctx = NewContext();
        SeedAccounts(ctx);
        // Prior-period receipt — must land in Cash's opening balance, not its period receipts.
        SeedEntry(ctx, "JV-0", new DateTime(2026, 7, 20), JournalEntryStatus.Posted, new[]
        {
            (CashId, 300m, 0m, (Guid?)null),
            (SalesId, 0m, 300m, (Guid?)SiteAId),
        });
        SeedEntry(ctx, "JV-1", new DateTime(2026, 8, 5), JournalEntryStatus.Posted, new[]
        {
            (CashId, 2000m, 0m, (Guid?)null),
            (SalesId, 0m, 2000m, (Guid?)SiteAId),
        });
        SeedEntry(ctx, "JV-2", new DateTime(2026, 8, 6), JournalEntryStatus.Posted, new[]
        {
            (ConveyanceId, 500m, 0m, (Guid?)SiteAId),
            (CashId, 0m, 500m, (Guid?)null),
        });
        SeedEntry(ctx, "JV-3", new DateTime(2026, 8, 7), JournalEntryStatus.Draft, new[]
        {
            (BankId, 1000m, 0m, (Guid?)null),
            (CashId, 0m, 1000m, (Guid?)null),
        });

        var report = await new AccountingReportService(ctx).GetReceiptsPaymentsAsync(August2026);

        var cash = report.Balances.Single(b => b.AccountName == "Cash");
        cash.Opening.Should().Be(1300m);          // 1000 opening + 300 prior receipt
        cash.Receipts.Should().Be(2000m);
        cash.Payments.Should().Be(1500m);         // 500 conveyance + 1000 transferred out
        cash.Closing.Should().Be(1800m);

        var bank = report.Balances.Single(b => b.AccountName == "Bank");
        bank.Opening.Should().Be(5000m);
        bank.Receipts.Should().Be(1000m);
        bank.Closing.Should().Be(6000m);

        report.TotalOpening.Should().Be(6300m);
        report.TotalClosing.Should().Be(7800m);
    }

    [Fact]
    public async Task Cancelled_entries_are_ignored()
    {
        using var ctx = NewContext();
        SeedAccounts(ctx);
        SeedEntry(ctx, "JV-1", new DateTime(2026, 8, 5), JournalEntryStatus.Cancelled, new[]
        {
            (CashId, 9999m, 0m, (Guid?)null),
            (SalesId, 0m, 9999m, (Guid?)SiteAId),
        });

        var report = await new AccountingReportService(ctx).GetReceiptsPaymentsAsync(August2026);

        report.Receipts.Should().BeEmpty();
        report.TotalReceiptsExclTransfers.Should().Be(0m);
        report.Balances.Single(b => b.AccountName == "Cash").Closing.Should().Be(1000m);
    }

    [Fact]
    public async Task A_bank_account_gl_is_treated_as_cash_bank_even_without_the_flag()
    {
        using var ctx = NewContext();
        SeedAccounts(ctx);
        var tradeBankId = Guid.NewGuid();
        ctx.Set<ChartOfAccount>().Add(new ChartOfAccount
        {
            Id = tradeBankId, AccountCode = "1200", AccountName = "Trade Bank",
            AccountGroupId = Guid.NewGuid(), AccountType = AccountGroupType.Asset,
            IsCashOrBank = false, OpeningBalance = 0m,
        });
        ctx.Set<BankAccount>().Add(new BankAccount
        {
            Id = Guid.NewGuid(), AccountNumber = "0001", AccountName = "Trade Bank",
            BankName = "Trade Bank Ltd", ChartOfAccountId = tradeBankId,
        });
        ctx.SaveChanges();
        SeedEntry(ctx, "JV-1", new DateTime(2026, 8, 5), JournalEntryStatus.Posted, new[]
        {
            (tradeBankId, 3000m, 0m, (Guid?)null),
            (SalesId, 0m, 3000m, (Guid?)SiteAId),
        });

        var report = await new AccountingReportService(ctx).GetReceiptsPaymentsAsync(August2026);

        report.Balances.Should().Contain(b => b.AccountName == "Trade Bank" && b.Receipts == 3000m);
        report.TotalReceiptsExclTransfers.Should().Be(3000m);
    }

    [Fact]
    public async Task No_cash_or_bank_accounts_yields_an_empty_report()
    {
        using var ctx = NewContext();
        ctx.Set<ChartOfAccount>().Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(), AccountCode = "4000", AccountName = "Sales",
            AccountGroupId = Guid.NewGuid(), AccountType = AccountGroupType.Income,
        });
        ctx.SaveChanges();

        var report = await new AccountingReportService(ctx).GetReceiptsPaymentsAsync(August2026);

        report.Balances.Should().BeEmpty();
        report.Receipts.Should().BeEmpty();
        report.Payments.Should().BeEmpty();
        report.Transfers.Should().BeEmpty();
    }
}
