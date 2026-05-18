using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Banking;
using uOrgHub.Accounts.Features.Banking;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Handlers;

public class BankingHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public BankingHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
    }

    public void Dispose() => _context.Dispose();

    // ─── Seed helpers ───────────────────────────────────────────────────────────

    private BankAccount SeedBankAccount(
        string accountNumber = "ACC-001",
        string accountName = "Main Account",
        string bankName = "Test Bank",
        bool isDeleted = false,
        decimal balance = 0m)
    {
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            AccountName = accountName,
            BankName = bankName,
            Currency = "BDT",
            OpeningBalance = balance,
            CurrentBalance = balance,
            IsActive = true,
            IsDeleted = isDeleted,
            ChartOfAccountId = Guid.NewGuid()
        };
        _context.Set<BankAccount>().Add(account);
        _context.SaveChanges();
        return account;
    }

    private BankTransaction SeedBankTransaction(
        BankAccount bankAccount,
        decimal amount = 1000m,
        BankTransactionType transactionType = BankTransactionType.Deposit,
        bool isDeleted = false,
        DateTime? transactionDate = null)
    {
        var tx = new BankTransaction
        {
            Id = Guid.NewGuid(),
            BankAccountId = bankAccount.Id,
            TransactionType = transactionType,
            TransactionDate = transactionDate ?? DateTime.UtcNow,
            Amount = amount,
            Description = "Test transaction",
            IsDeleted = isDeleted
        };
        _context.Set<BankTransaction>().Add(tx);
        _context.SaveChanges();
        return tx;
    }

    private CreateBankAccountDto ValidCreateDto(string accountNumber = "ACC-NEW") => new()
    {
        AccountNumber = accountNumber,
        AccountName = "New Account",
        BankName = "Sample Bank",
        BranchName = "Head Office",
        RoutingNumber = "123456789",
        Currency = "BDT",
        OpeningBalance = 1000m,
        ChartOfAccountId = Guid.NewGuid()
    };

    // ─── CreateBankAccountCommandHandler ────────────────────────────────────────

    [Fact]
    public async Task Create_saves_bank_account_with_correct_fields()
    {
        var handler = new CreateBankAccountCommandHandler(_context);
        var dto = ValidCreateDto("ACC-001");

        var result = await handler.Handle(new CreateBankAccountCommand(dto), default);

        result.AccountNumber.Should().Be("ACC-001");
        result.AccountName.Should().Be("New Account");
        result.BankName.Should().Be("Sample Bank");
        result.IsActive.Should().BeTrue();
        result.CurrentBalance.Should().Be(dto.OpeningBalance);
        _context.Set<BankAccount>().Count(b => !b.IsDeleted).Should().Be(1);
    }

    [Fact]
    public async Task Create_throws_when_account_number_already_exists()
    {
        SeedBankAccount("ACC-DUP");
        var handler = new CreateBankAccountCommandHandler(_context);

        var act = () => handler.Handle(new CreateBankAccountCommand(ValidCreateDto("ACC-DUP")), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*ACC-DUP*");
    }

    [Fact]
    public async Task Create_allows_same_number_as_soft_deleted_account()
    {
        SeedBankAccount("ACC-REUSE", isDeleted: true);
        var handler = new CreateBankAccountCommandHandler(_context);

        var result = await handler.Handle(new CreateBankAccountCommand(ValidCreateDto("ACC-REUSE")), default);

        result.AccountNumber.Should().Be("ACC-REUSE");
    }

    [Fact]
    public async Task Create_sets_current_balance_to_opening_balance()
    {
        var handler = new CreateBankAccountCommandHandler(_context);
        var dto = ValidCreateDto("ACC-BAL");
        dto.OpeningBalance = 5000m;

        var result = await handler.Handle(new CreateBankAccountCommand(dto), default);

        result.OpeningBalance.Should().Be(5000m);
        result.CurrentBalance.Should().Be(5000m);
    }

    // ─── UpdateBankAccountCommandHandler ────────────────────────────────────────

    [Fact]
    public async Task Update_modifies_account_name_and_bank_name()
    {
        var account = SeedBankAccount("ACC-UPD", "Old Name", "Old Bank");
        var handler = new UpdateBankAccountCommandHandler(_context);
        var dto = new UpdateBankAccountDto
        {
            AccountName = "Updated Name",
            BankName = "Updated Bank",
            BranchName = "New Branch",
            RoutingNumber = "987654321",
            IsActive = false
        };

        var result = await handler.Handle(new UpdateBankAccountCommand(account.Id, dto), default);

        result.AccountName.Should().Be("Updated Name");
        result.BankName.Should().Be("Updated Bank");
        result.BranchName.Should().Be("New Branch");
        result.RoutingNumber.Should().Be("987654321");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Update_throws_NotFoundException_for_missing_account()
    {
        var handler = new UpdateBankAccountCommandHandler(_context);
        var dto = new UpdateBankAccountDto
        {
            AccountName = "X", BankName = "Y", IsActive = true
        };

        var act = () => handler.Handle(new UpdateBankAccountCommand(Guid.NewGuid(), dto), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_throws_NotFoundException_for_deleted_account()
    {
        var account = SeedBankAccount("ACC-DEL", isDeleted: true);
        var handler = new UpdateBankAccountCommandHandler(_context);
        var dto = new UpdateBankAccountDto
        {
            AccountName = "X", BankName = "Y", IsActive = true
        };

        var act = () => handler.Handle(new UpdateBankAccountCommand(account.Id, dto), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── GetBankAccountsQueryHandler ────────────────────────────────────────────

    [Fact]
    public async Task GetAll_excludes_deleted_accounts()
    {
        SeedBankAccount("ACC-A", "Alpha");
        SeedBankAccount("ACC-B", "Beta");
        SeedBankAccount("ACC-DEL", "Deleted", isDeleted: true);
        var handler = new GetBankAccountsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankAccountsQuery(new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(a => a.AccountName.Should().NotBe("Deleted"));
    }

    [Fact]
    public async Task GetAll_filters_by_account_name_search()
    {
        SeedBankAccount("ACC-001", "Payroll Account", "Bank A");
        SeedBankAccount("ACC-002", "Savings Account", "Bank B");
        SeedBankAccount("ACC-003", "Petty Cash", "Bank C");
        var handler = new GetBankAccountsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankAccountsQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "Savings" }), default);

        result.TotalCount.Should().Be(1);
        result.Items.First().AccountName.Should().Be("Savings Account");
    }

    [Fact]
    public async Task GetAll_filters_by_account_number_search()
    {
        SeedBankAccount("ACC-MATCH-001", "Account One");
        SeedBankAccount("ACC-MATCH-002", "Account Two");
        SeedBankAccount("XYZ-999", "Other Account");
        var handler = new GetBankAccountsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankAccountsQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "ACC-MATCH" }), default);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAll_filters_by_bank_name_search()
    {
        SeedBankAccount("ACC-001", "Account One", "Dutch-Bangla Bank");
        SeedBankAccount("ACC-002", "Account Two", "Dutch-Bangla Bank");
        SeedBankAccount("ACC-003", "Account Three", "Islami Bank");
        var handler = new GetBankAccountsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankAccountsQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "Dutch-Bangla" }), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(a => a.BankName.Should().Be("Dutch-Bangla Bank"));
    }

    [Fact]
    public async Task GetAll_sorts_ascending_by_name()
    {
        SeedBankAccount("ACC-003", "Zebra Account");
        SeedBankAccount("ACC-001", "Alpha Account");
        SeedBankAccount("ACC-002", "Middle Account");
        var handler = new GetBankAccountsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankAccountsQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = false }), default);

        result.Items.Select(a => a.AccountName).Should()
            .BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAll_sorts_descending_by_name()
    {
        SeedBankAccount("ACC-003", "Zebra Account");
        SeedBankAccount("ACC-001", "Alpha Account");
        SeedBankAccount("ACC-002", "Middle Account");
        var handler = new GetBankAccountsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankAccountsQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = true }), default);

        result.Items.Select(a => a.AccountName).Should()
            .BeInDescendingOrder();
    }

    [Fact]
    public async Task GetAll_paginates_correctly()
    {
        for (var i = 1; i <= 5; i++)
            SeedBankAccount($"ACC-{i:000}", $"Account {i:000}");
        var handler = new GetBankAccountsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankAccountsQuery(new PaginationRequest { Page = 2, PageSize = 2 }), default);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    // ─── GetBankAccountByIdQueryHandler ─────────────────────────────────────────

    [Fact]
    public async Task GetById_returns_correct_bank_account()
    {
        var account = SeedBankAccount("ACC-X1", "Target Account", "Target Bank");
        var handler = new GetBankAccountByIdQueryHandler(_context);

        var result = await handler.Handle(new GetBankAccountByIdQuery(account.Id), default);

        result.Id.Should().Be(account.Id);
        result.AccountNumber.Should().Be("ACC-X1");
        result.AccountName.Should().Be("Target Account");
        result.BankName.Should().Be("Target Bank");
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_missing()
    {
        var handler = new GetBankAccountByIdQueryHandler(_context);

        var act = () => handler.Handle(new GetBankAccountByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_deleted()
    {
        var account = SeedBankAccount("ACC-DEL2", isDeleted: true);
        var handler = new GetBankAccountByIdQueryHandler(_context);

        var act = () => handler.Handle(new GetBankAccountByIdQuery(account.Id), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── CreateBankTransactionCommandHandler ────────────────────────────────────

    [Fact]
    public async Task CreateTransaction_updates_bank_account_balance()
    {
        var account = SeedBankAccount("ACC-TXN", balance: 10_000m);
        var handler = new CreateBankTransactionCommandHandler(_context);
        var dto = new CreateBankTransactionDto
        {
            BankAccountId = account.Id,
            TransactionType = BankTransactionType.Deposit,
            TransactionDate = DateTime.UtcNow,
            Amount = 2_500m,
            Description = "Customer payment received"
        };

        var result = await handler.Handle(new CreateBankTransactionCommand(dto), default);

        result.Amount.Should().Be(2_500m);
        result.BankAccountId.Should().Be(account.Id);
        result.BankAccountName.Should().Be("Main Account");

        var updatedAccount = await _context.Set<BankAccount>().FindAsync(account.Id);
        updatedAccount!.CurrentBalance.Should().Be(12_500m);
    }

    [Fact]
    public async Task CreateTransaction_throws_NotFoundException_for_missing_bank_account()
    {
        var handler = new CreateBankTransactionCommandHandler(_context);
        var dto = new CreateBankTransactionDto
        {
            BankAccountId = Guid.NewGuid(),
            TransactionType = BankTransactionType.Withdrawal,
            TransactionDate = DateTime.UtcNow,
            Amount = 500m,
            Description = "ATM withdrawal"
        };

        var act = () => handler.Handle(new CreateBankTransactionCommand(dto), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── GetBankTransactionsQueryHandler ────────────────────────────────────────

    [Fact]
    public async Task GetTransactions_returns_only_transactions_for_given_account()
    {
        var accountA = SeedBankAccount("ACC-A1", "Account A");
        var accountB = SeedBankAccount("ACC-B1", "Account B");
        SeedBankTransaction(accountA, 500m);
        SeedBankTransaction(accountA, 300m);
        SeedBankTransaction(accountB, 200m);
        var handler = new GetBankTransactionsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankTransactionsQuery(accountA.Id, new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(t => t.BankAccountId.Should().Be(accountA.Id));
    }

    [Fact]
    public async Task GetTransactions_excludes_deleted_transactions()
    {
        var account = SeedBankAccount("ACC-TXN2");
        SeedBankTransaction(account, 1000m, BankTransactionType.Deposit);
        SeedBankTransaction(account, 500m, BankTransactionType.Withdrawal);
        SeedBankTransaction(account, 200m, BankTransactionType.Fee, isDeleted: true);
        var handler = new GetBankTransactionsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankTransactionsQuery(account.Id, new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTransactions_paginates_correctly()
    {
        var account = SeedBankAccount("ACC-PAG");
        var baseDate = DateTime.UtcNow;
        for (var i = 0; i < 5; i++)
            SeedBankTransaction(account, 100m * (i + 1), transactionDate: baseDate.AddMinutes(-i));
        var handler = new GetBankTransactionsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBankTransactionsQuery(account.Id, new PaginationRequest { Page = 2, PageSize = 2 }), default);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }
}
