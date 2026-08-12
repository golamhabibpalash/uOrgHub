using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Validators;
using uOrgHub.Accounts.Features.Voucher;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Data;

namespace uOrgHub.Tests.Accounts.Handlers;

/// <summary>
/// Covers the step each voucher's double entry is generated at. The entry has to exist from
/// submit onwards — that is the point the voucher stops being editable, so it is the earliest
/// moment an entry is guaranteed to still match the voucher it came from.
///
/// Runs against the real <see cref="JournalEntryService"/> rather than a mock, because what is
/// being asserted is the shape of the entry it produces: two balanced lines, right way round.
/// </summary>
public class VoucherWorkflowHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly JournalEntryService _jeService;

    public VoucherWorkflowHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _jeService = new JournalEntryService(
            _context,
            new JournalEntryRepository(_context),
            new CreateJournalEntryValidator(),
            new UpdateJournalEntryValidator());
    }

    public void Dispose() => _context.Dispose();

    private ChartOfAccount SeedAccount(string code, string name, AccountGroupType type)
    {
        var account = new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountCode = code,
            AccountName = name,
            AccountType = type,
            IsActive = true,
            AllowDirectEntry = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ChartOfAccount>().Add(account);
        _context.SaveChanges();
        return account;
    }

    private CostCenter SeedCostCenter()
    {
        var cc = new CostCenter
        {
            Id = Guid.NewGuid(),
            Code = "CC-001",
            Name = "Head Office",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<CostCenter>().Add(cc);
        _context.SaveChanges();
        return cc;
    }

    /// <summary>
    /// A draft voucher of the given type, with the accounts already on the sides the guard would
    /// have put them on at creation time.
    /// </summary>
    private Voucher SeedDraftVoucher(VoucherType type, Guid debitAccountId, Guid creditAccountId, decimal amount = 5000m)
    {
        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            VoucherNumber = $"{VoucherAccountRules.NumberPrefix(type)}-2026-0001",
            VoucherType = type,
            VoucherDate = new DateTime(2026, 8, 12),
            Description = "Site materials",
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId,
            CostCenterId = SeedCostCenter().Id,
            Amount = amount,
            Status = VoucherStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "rafiq"
        };
        _context.Set<Voucher>().Add(voucher);
        _context.SaveChanges();
        return voucher;
    }

    private Task<uOrgHub.Accounts.DTOs.Voucher.VoucherResponseDto> Submit(Guid id)
        => new SubmitVoucherCommandHandler(_context, _jeService)
            .Handle(new SubmitVoucherCommand(id, "rafiq"), default);

    private JournalEntry LoadEntry(Guid journalEntryId)
        => _context.Set<JournalEntry>()
            .Include(x => x.Lines)
            .First(x => x.Id == journalEntryId);

    /// <summary>
    /// The three voucher types differ only in which accounts sit on each side; all three must
    /// produce the same balanced two-line entry on submit.
    /// </summary>
    [Theory]
    [InlineData(VoucherType.Debit, AccountGroupType.Expense, AccountGroupType.Asset)]
    [InlineData(VoucherType.Credit, AccountGroupType.Asset, AccountGroupType.Income)]
    [InlineData(VoucherType.Contra, AccountGroupType.Asset, AccountGroupType.Asset)]
    public async Task Submit_creates_the_balanced_double_entry(
        VoucherType type, AccountGroupType debitType, AccountGroupType creditType)
    {
        var debit = SeedAccount("5001", "Construction Materials", debitType);
        var credit = SeedAccount("1001", "Cash at Bank", creditType);
        var voucher = SeedDraftVoucher(type, debit.Id, credit.Id);

        var result = await Submit(voucher.Id);

        result.Status.Should().Be(VoucherStatus.Submitted);
        result.JournalEntryId.Should().NotBeNull();

        var entry = LoadEntry(result.JournalEntryId!.Value);
        entry.Status.Should().Be(JournalEntryStatus.Draft);
        entry.TotalDebit.Should().Be(5000m);
        entry.TotalCredit.Should().Be(5000m);
        entry.ReferenceNumber.Should().Be(voucher.VoucherNumber);

        var debitLine = entry.Lines.Single(l => l.AccountId == debit.Id);
        debitLine.DebitAmount.Should().Be(5000m);
        debitLine.CreditAmount.Should().Be(0m);

        var creditLine = entry.Lines.Single(l => l.AccountId == credit.Id);
        creditLine.CreditAmount.Should().Be(5000m);
        creditLine.DebitAmount.Should().Be(0m);
    }

    /// <summary>
    /// Both lines must carry the cost center — it is the only thing tying the amount back to a
    /// project, since project reporting reads posted lines by cost center.
    /// </summary>
    [Fact]
    public async Task Submit_stamps_the_cost_center_on_both_lines()
    {
        var debit = SeedAccount("5001", "Construction Materials", AccountGroupType.Expense);
        var credit = SeedAccount("1001", "Cash at Bank", AccountGroupType.Asset);
        var voucher = SeedDraftVoucher(VoucherType.Debit, debit.Id, credit.Id);

        var result = await Submit(voucher.Id);

        var entry = LoadEntry(result.JournalEntryId!.Value);
        entry.Lines.Should().OnlyContain(l => l.CostCenterId == voucher.CostCenterId);
        entry.CreatedBy.Should().Be("rafiq");
    }

    [Fact]
    public async Task Approve_reuses_the_entry_created_on_submit()
    {
        var debit = SeedAccount("5001", "Construction Materials", AccountGroupType.Expense);
        var credit = SeedAccount("1001", "Cash at Bank", AccountGroupType.Asset);
        var voucher = SeedDraftVoucher(VoucherType.Debit, debit.Id, credit.Id);

        var submitted = await Submit(voucher.Id);
        var approved = await new ApproveVoucherCommandHandler(_context, _jeService)
            .Handle(new ApproveVoucherCommand(voucher.Id, "karim"), default);

        approved.Status.Should().Be(VoucherStatus.Approved);
        approved.JournalEntryId.Should().Be(submitted.JournalEntryId);
        _context.Set<JournalEntry>().Count(x => !x.IsDeleted).Should().Be(1);
    }

    /// <summary>
    /// A voucher submitted before entries were generated on submit still has to get one, or it
    /// would reach approval with nothing to post.
    /// </summary>
    [Fact]
    public async Task Approve_creates_the_entry_when_an_older_voucher_has_none()
    {
        var debit = SeedAccount("5001", "Construction Materials", AccountGroupType.Expense);
        var credit = SeedAccount("1001", "Cash at Bank", AccountGroupType.Asset);
        var voucher = SeedDraftVoucher(VoucherType.Debit, debit.Id, credit.Id);
        voucher.Status = VoucherStatus.Submitted;
        _context.SaveChanges();

        var approved = await new ApproveVoucherCommandHandler(_context, _jeService)
            .Handle(new ApproveVoucherCommand(voucher.Id, "karim"), default);

        approved.JournalEntryId.Should().NotBeNull();
        LoadEntry(approved.JournalEntryId!.Value).TotalDebit.Should().Be(5000m);
    }

    [Fact]
    public async Task Post_posts_the_entry_and_moves_the_account_balances()
    {
        var debit = SeedAccount("5001", "Construction Materials", AccountGroupType.Expense);
        var credit = SeedAccount("1001", "Cash at Bank", AccountGroupType.Asset);
        var voucher = SeedDraftVoucher(VoucherType.Debit, debit.Id, credit.Id);

        await Submit(voucher.Id);
        await new ApproveVoucherCommandHandler(_context, _jeService)
            .Handle(new ApproveVoucherCommand(voucher.Id, "karim"), default);
        var posted = await new PostVoucherCommandHandler(_context, _jeService)
            .Handle(new PostVoucherCommand(voucher.Id, "karim"), default);

        posted.Status.Should().Be(VoucherStatus.Posted);
        LoadEntry(posted.JournalEntryId!.Value).Status.Should().Be(JournalEntryStatus.Posted);

        // Expense rises by the amount, the bank it was paid from falls by the same.
        _context.Set<ChartOfAccount>().First(a => a.Id == debit.Id).CurrentBalance.Should().Be(5000m);
        _context.Set<ChartOfAccount>().First(a => a.Id == credit.Id).CurrentBalance.Should().Be(-5000m);
    }

    /// <summary>
    /// Rejecting has to take the draft entry with it, or the journal fills up with entries for
    /// money that was never approved to move.
    /// </summary>
    [Fact]
    public async Task Reject_removes_the_draft_entry_and_unlinks_it()
    {
        var debit = SeedAccount("5001", "Construction Materials", AccountGroupType.Expense);
        var credit = SeedAccount("1001", "Cash at Bank", AccountGroupType.Asset);
        var voucher = SeedDraftVoucher(VoucherType.Debit, debit.Id, credit.Id);

        var submitted = await Submit(voucher.Id);
        var rejected = await new RejectVoucherCommandHandler(_context, _jeService)
            .Handle(new RejectVoucherCommand(voucher.Id, "Wrong cost center", "karim"), default);

        rejected.Status.Should().Be(VoucherStatus.Rejected);
        rejected.JournalEntryId.Should().BeNull();
        _context.Set<JournalEntry>().First(x => x.Id == submitted.JournalEntryId).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Cancel_removes_the_draft_entry_and_unlinks_it()
    {
        var debit = SeedAccount("5001", "Construction Materials", AccountGroupType.Expense);
        var credit = SeedAccount("1001", "Cash at Bank", AccountGroupType.Asset);
        var voucher = SeedDraftVoucher(VoucherType.Debit, debit.Id, credit.Id);

        var submitted = await Submit(voucher.Id);
        var cancelled = await new CancelVoucherCommandHandler(_context, _jeService)
            .Handle(new CancelVoucherCommand(voucher.Id), default);

        cancelled.Status.Should().Be(VoucherStatus.Cancelled);
        cancelled.JournalEntryId.Should().BeNull();
        _context.Set<JournalEntry>().First(x => x.Id == submitted.JournalEntryId).IsDeleted.Should().BeTrue();
    }
}
