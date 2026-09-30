using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using uOrgHub.Accounts.DTOs.Validators;
using uOrgHub.Accounts.DTOs.Voucher;
using uOrgHub.Accounts.Features.Voucher;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Repositories;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Tests.Accounts.Handlers;

/// <summary>
/// "Correct this voucher": a posted voucher is reversed by a mirror entry and replaced by a draft
/// copy. Runs against the real <see cref="JournalEntryService"/> so the ledger balances it moves
/// are the ones asserted.
/// </summary>
public class VoucherCorrectionHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly JournalEntryService _jeService;
    private readonly IDocumentNumberingService _numbering;

    private static readonly DateTime VoucherDate = new(2026, 8, 12);

    public VoucherCorrectionHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _jeService = new JournalEntryService(
            _context,
            new JournalEntryRepository(_context),
            new CreateJournalEntryValidator(),
            new UpdateJournalEntryValidator(),
            new JournalEntrySourceService(_context));

        var numbering = new Mock<IDocumentNumberingService>();
        var seq = 100;
        numbering.Setup(x => x.GenerateNextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
            .ReturnsAsync((string _, string prefix, int? _, int? _) => $"{prefix}-2026-{++seq:0000}");
        _numbering = numbering.Object;

        _context.Set<FiscalYear>().Add(new FiscalYear
        {
            Id = Guid.NewGuid(),
            Name = "FY 2026-27",
            StartDate = new DateTime(2026, 7, 1),
            EndDate = new DateTime(2027, 6, 30),
            Status = FiscalYearStatus.Active
        });
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    private ChartOfAccount SeedAccount(string code, string name, AccountGroupType type)
    {
        var account = new ChartOfAccount
        {
            Id = Guid.NewGuid(), AccountCode = code, AccountName = name, AccountType = type,
            IsActive = true, AllowDirectEntry = true, CreatedAt = DateTime.UtcNow
        };
        _context.Set<ChartOfAccount>().Add(account);
        _context.SaveChanges();
        return account;
    }

    /// <summary>A Debit voucher for 500 taken through submit → approve → post.</summary>
    private async Task<(Voucher Voucher, ChartOfAccount Expense, ChartOfAccount Bank)> SeedPostedVoucherAsync()
    {
        var expense = SeedAccount("5001", "Site Materials", AccountGroupType.Expense);
        var bank = SeedAccount("1101", "Bank", AccountGroupType.Asset);
        var cc = new CostCenter { Id = Guid.NewGuid(), Code = "CC-1", Name = "Head Office", IsActive = true };
        _context.Set<CostCenter>().Add(cc);

        var voucher = new Voucher
        {
            Id = Guid.NewGuid(),
            VoucherNumber = "DR-2026-0001",
            VoucherType = VoucherType.Debit,
            VoucherDate = VoucherDate,
            Description = "Cement",
            DebitAccountId = expense.Id,
            CreditAccountId = bank.Id,
            CostCenterId = cc.Id,
            Amount = 500m,
            Status = VoucherStatus.Draft,
            CreatedBy = "rafiq"
        };
        _context.Set<Voucher>().Add(voucher);
        _context.SaveChanges();

        await new SubmitVoucherCommandHandler(_context, _jeService).Handle(new SubmitVoucherCommand(voucher.Id, "rafiq"), default);
        await new ApproveVoucherCommandHandler(_context, _jeService).Handle(new ApproveVoucherCommand(voucher.Id, "boss"), default);
        await new PostVoucherCommandHandler(_context, _jeService).Handle(new PostVoucherCommand(voucher.Id, "boss"), default);
        return (voucher, expense, bank);
    }

    private Task<VoucherResponseDto> Correct(Guid id, string reason = "Should have been 560", DateTime? date = null)
        => new ReverseVoucherCommandHandler(_context, _jeService, _numbering)
            .Handle(new ReverseVoucherCommand(id, new ReverseVoucherDto { Reason = reason, ReversalDate = date }, "auditor"), default);

    [Fact]
    public async Task Correct_posts_a_mirror_entry_that_nets_the_ledger_back_to_zero()
    {
        var (voucher, expense, bank) = await SeedPostedVoucherAsync();
        _context.Find<ChartOfAccount>(expense.Id)!.CurrentBalance.Should().Be(500m);

        await Correct(voucher.Id, date: new DateTime(2026, 9, 30));

        var original = _context.Set<Voucher>().Include(x => x.ReversalJournalEntry).ThenInclude(j => j!.Lines).Single(x => x.Id == voucher.Id);
        original.Status.Should().Be(VoucherStatus.Reversed);
        original.ReversedBy.Should().Be("auditor");
        original.ReversalReason.Should().Be("Should have been 560");

        var mirror = original.ReversalJournalEntry!;
        mirror.Status.Should().Be(JournalEntryStatus.Posted);
        mirror.EntryDate.Should().Be(new DateTime(2026, 9, 30));
        mirror.Lines.Single(l => l.DebitAmount > 0).AccountId.Should().Be(bank.Id);
        mirror.Lines.Single(l => l.CreditAmount > 0).AccountId.Should().Be(expense.Id);
        mirror.Lines.Should().OnlyContain(l => l.CostCenterId == voucher.CostCenterId);

        // The original entry stays posted in its own period; the pair nets to nothing.
        _context.Find<JournalEntry>(voucher.JournalEntryId!.Value)!.Status.Should().Be(JournalEntryStatus.Posted);
        _context.Find<ChartOfAccount>(expense.Id)!.CurrentBalance.Should().Be(0m);
        _context.Find<ChartOfAccount>(bank.Id)!.CurrentBalance.Should().Be(0m);
    }

    [Fact]
    public async Task Correct_returns_a_linked_draft_copy_to_fix()
    {
        var (voucher, _, _) = await SeedPostedVoucherAsync();

        var draft = await Correct(voucher.Id, date: new DateTime(2026, 9, 30));

        draft.Id.Should().NotBe(voucher.Id);
        draft.Status.Should().Be(VoucherStatus.Draft);
        draft.CorrectsVoucherId.Should().Be(voucher.Id);
        draft.CorrectsVoucherNumber.Should().Be("DR-2026-0001");
        draft.Amount.Should().Be(500m, "the user edits it to the right amount before submitting");
        draft.VoucherDate.Should().Be(new DateTime(2026, 9, 30));
        draft.DebitAccountId.Should().Be(voucher.DebitAccountId);
        draft.JournalEntryId.Should().BeNull();
    }

    [Fact]
    public async Task Correct_defaults_the_reversal_date_to_today()
    {
        var (voucher, _, _) = await SeedPostedVoucherAsync();
        // Today must sit in an open fiscal year for the default to be usable.
        _context.Set<FiscalYear>().Add(new FiscalYear
        {
            Id = Guid.NewGuid(), Name = "Current", Status = FiscalYearStatus.Active,
            StartDate = DateTime.UtcNow.Date.AddDays(-1), EndDate = DateTime.UtcNow.Date.AddDays(1)
        });
        _context.SaveChanges();

        await Correct(voucher.Id);

        var original = _context.Set<Voucher>().Include(x => x.ReversalJournalEntry).Single(x => x.Id == voucher.Id);
        original.ReversalJournalEntry!.EntryDate.Should().Be(DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task Correct_requires_a_reason()
    {
        var (voucher, _, _) = await SeedPostedVoucherAsync();

        var act = () => Correct(voucher.Id, reason: "  ", date: new DateTime(2026, 9, 30));

        await act.Should().ThrowAsync<ValidationException>();
        _context.Find<Voucher>(voucher.Id)!.Status.Should().Be(VoucherStatus.Posted);
    }

    [Fact]
    public async Task Correct_refuses_a_voucher_that_is_not_posted()
    {
        var (voucher, _, _) = await SeedPostedVoucherAsync();
        await Correct(voucher.Id, date: new DateTime(2026, 9, 30));

        var again = () => Correct(voucher.Id, date: new DateTime(2026, 9, 30));

        await again.Should().ThrowAsync<AppException>().WithMessage("*Only posted vouchers*");
    }

    [Fact]
    public async Task Correct_refuses_a_reversal_date_before_the_voucher_date()
    {
        var (voucher, _, _) = await SeedPostedVoucherAsync();

        var act = () => Correct(voucher.Id, date: VoucherDate.AddDays(-1));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Correct_refuses_a_reversal_date_in_a_closed_fiscal_year()
    {
        var (voucher, _, _) = await SeedPostedVoucherAsync();
        _context.Set<FiscalYear>().Single().Status = FiscalYearStatus.Closed;
        _context.SaveChanges();

        var act = () => Correct(voucher.Id, date: new DateTime(2026, 9, 30));

        await act.Should().ThrowAsync<ValidationException>();
        _context.Find<Voucher>(voucher.Id)!.Status.Should().Be(VoucherStatus.Posted);
    }

    [Fact]
    public async Task Correct_refuses_a_payment_generated_voucher()
    {
        var (voucher, _, _) = await SeedPostedVoucherAsync();
        _context.Set<Payment>().Add(new Payment
        {
            Id = Guid.NewGuid(), PaymentNumber = "PMT-0001", Amount = 500m, FiscalYearId = Guid.NewGuid(),
            VoucherId = voucher.Id
        });
        _context.SaveChanges();

        var act = () => Correct(voucher.Id, date: new DateTime(2026, 9, 30));

        await act.Should().ThrowAsync<AppException>().WithMessage("*payment PMT-0001*");
    }

    [Fact]
    public async Task A_reversed_voucher_cannot_be_cancelled()
    {
        var (voucher, _, _) = await SeedPostedVoucherAsync();
        await Correct(voucher.Id, date: new DateTime(2026, 9, 30));

        var act = () => new CancelVoucherCommandHandler(_context, _jeService).Handle(new CancelVoucherCommand(voucher.Id), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*Reversed*");
    }

    [Fact]
    public async Task The_reversal_entry_cannot_be_cancelled_from_the_journal_screen()
    {
        var (voucher, _, _) = await SeedPostedVoucherAsync();
        await Correct(voucher.Id, date: new DateTime(2026, 9, 30));
        var mirrorId = _context.Find<Voucher>(voucher.Id)!.ReversalJournalEntryId!.Value;

        var act = () => new JournalEntrySourceService(_context).EnsureNotDocumentOwnedAsync(mirrorId, "cancelled");

        await act.Should().ThrowAsync<AppException>().WithMessage("*Voucher DR-2026-0001*");
    }
}
