using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Budget;
using uOrgHub.Accounts.Features.Budget;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Handlers;

public class BudgetHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public BudgetHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
    }

    public void Dispose() => _context.Dispose();

    // -----------------------------------------------------------------------
    // Seed helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Seeds a Budget (with no lines) directly into the in-memory database.
    /// Uses a separate SaveChanges so existing tests are not affected by the
    /// EF InMemory UPDATE+INSERT bug. Lines can be seeded separately via
    /// SeedBudgetLine if needed.
    /// </summary>
    private Budget SeedBudget(
        string name = "Test Budget",
        BudgetStatus status = BudgetStatus.Draft,
        bool isDeleted = false,
        Guid? fiscalYearId = null)
    {
        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Seeded description",
            Status = status,
            TotalAmount = 0,
            FiscalYearId = fiscalYearId ?? Guid.NewGuid(),
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<Budget>().Add(budget);
        _context.SaveChanges();
        return budget;
    }

    /// <summary>
    /// Seeds a BudgetLine belonging to the given budget.
    /// Must be called after SeedBudget so Budget is already in Unchanged state,
    /// making this a pure INSERT with no concurrent UPDATE – safe for InMemory.
    /// </summary>
    private BudgetLine SeedBudgetLine(Guid budgetId, decimal plannedAmount = 1000m, int period = 1)
    {
        var line = new BudgetLine
        {
            Id = Guid.NewGuid(),
            BudgetId = budgetId,
            AccountId = Guid.NewGuid(), // phantom FK – no ChartOfAccount entity needed
            Period = period,
            PlannedAmount = plannedAmount,
            ActualAmount = 0,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<BudgetLine>().Add(line);
        _context.SaveChanges();
        return line;
    }

    /// <summary>
    /// Returns a valid CreateBudgetDto with two lines summing to 3 000.
    /// </summary>
    private static CreateBudgetDto ValidCreateDto(string name = "Budget 2026") => new()
    {
        Name = name,
        Description = "Annual budget",
        FiscalYearId = Guid.NewGuid(),
        CostCenterId = null,
        Lines = new List<CreateBudgetLineDto>
        {
            new() { AccountId = Guid.NewGuid(), Period = 1, PlannedAmount = 1_000m },
            new() { AccountId = Guid.NewGuid(), Period = 2, PlannedAmount = 2_000m }
        }
    };

    // -----------------------------------------------------------------------
    // CreateBudgetCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Create_saves_budget_with_draft_status()
    {
        var handler = new CreateBudgetCommandHandler(_context);

        var result = await handler.Handle(
            new CreateBudgetCommand(ValidCreateDto("Budget 2026")), default);

        result.Name.Should().Be("Budget 2026");
        result.Status.Should().Be(BudgetStatus.Draft);

        var saved = await _context.Set<Budget>().FirstOrDefaultAsync(b => b.Id == result.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(BudgetStatus.Draft);
    }

    [Fact]
    public async Task Create_calculates_total_amount_from_lines()
    {
        var dto = new CreateBudgetDto
        {
            Name = "Line Sum Budget",
            FiscalYearId = Guid.NewGuid(),
            Lines = new List<CreateBudgetLineDto>
            {
                new() { AccountId = Guid.NewGuid(), Period = 1, PlannedAmount = 1_000m },
                new() { AccountId = Guid.NewGuid(), Period = 2, PlannedAmount = 2_000m }
            }
        };

        var handler = new CreateBudgetCommandHandler(_context);
        var result = await handler.Handle(new CreateBudgetCommand(dto), default);

        result.TotalAmount.Should().Be(3_000m);
        result.Lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task Create_with_empty_lines_has_zero_total()
    {
        var dto = new CreateBudgetDto
        {
            Name = "Empty Lines Budget",
            FiscalYearId = Guid.NewGuid(),
            Lines = new List<CreateBudgetLineDto>()
        };

        var handler = new CreateBudgetCommandHandler(_context);
        var result = await handler.Handle(new CreateBudgetCommand(dto), default);

        result.TotalAmount.Should().Be(0m);
        result.Lines.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // UpdateBudgetCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Update_draft_budget_modifies_name_and_description()
    {
        var budget = SeedBudget("Original Name", BudgetStatus.Draft);
        var handler = new UpdateBudgetCommandHandler(_context);

        var dto = new UpdateBudgetDto
        {
            Name = "Updated Name",
            Description = "Updated description",
            CostCenterId = null
        };

        var result = await handler.Handle(new UpdateBudgetCommand(budget.Id, dto), default);

        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated description");

        var saved = await _context.Set<Budget>().FindAsync(budget.Id);
        saved!.Name.Should().Be("Updated Name");
        saved.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_non_draft_budget_throws_AppException()
    {
        var budget = SeedBudget("Approved Budget", BudgetStatus.Approved);
        var handler = new UpdateBudgetCommandHandler(_context);

        var dto = new UpdateBudgetDto { Name = "Should Fail", Description = null };

        Func<Task> act = () => handler.Handle(new UpdateBudgetCommand(budget.Id, dto), default);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage("Only draft budgets can be updated.");
    }

    [Fact]
    public async Task Update_throws_NotFoundException_for_missing_budget()
    {
        var handler = new UpdateBudgetCommandHandler(_context);
        var missingId = Guid.NewGuid();

        var dto = new UpdateBudgetDto { Name = "Ghost Budget" };

        Func<Task> act = () => handler.Handle(new UpdateBudgetCommand(missingId, dto), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -----------------------------------------------------------------------
    // ApproveBudgetCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Approve_changes_status_to_Approved()
    {
        var budget = SeedBudget("Draft To Approve", BudgetStatus.Draft);
        var handler = new ApproveBudgetCommandHandler(_context);

        var result = await handler.Handle(new ApproveBudgetCommand(budget.Id), default);

        result.Status.Should().Be(BudgetStatus.Approved);

        var saved = await _context.Set<Budget>().FindAsync(budget.Id);
        saved!.Status.Should().Be(BudgetStatus.Approved);
        saved.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Approve_non_draft_budget_throws_AppException()
    {
        var budget = SeedBudget("Active Budget", BudgetStatus.Active);
        var handler = new ApproveBudgetCommandHandler(_context);

        Func<Task> act = () => handler.Handle(new ApproveBudgetCommand(budget.Id), default);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage("Only draft budgets can be approved.");
    }

    [Fact]
    public async Task Approve_throws_NotFoundException_for_missing_budget()
    {
        var handler = new ApproveBudgetCommandHandler(_context);
        var missingId = Guid.NewGuid();

        Func<Task> act = () => handler.Handle(new ApproveBudgetCommand(missingId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -----------------------------------------------------------------------
    // DeleteBudgetCommandHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Delete_soft_deletes_draft_budget()
    {
        var budget = SeedBudget("Draft To Delete", BudgetStatus.Draft);
        var handler = new DeleteBudgetCommandHandler(_context);

        var result = await handler.Handle(new DeleteBudgetCommand(budget.Id), default);

        result.Should().Be(Unit.Value);

        var saved = await _context.Set<Budget>().FindAsync(budget.Id);
        saved!.IsDeleted.Should().BeTrue();
        saved.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_soft_deletes_cancelled_budget()
    {
        var budget = SeedBudget("Cancelled Budget", BudgetStatus.Cancelled);
        var handler = new DeleteBudgetCommandHandler(_context);

        var result = await handler.Handle(new DeleteBudgetCommand(budget.Id), default);

        result.Should().Be(Unit.Value);

        var saved = await _context.Set<Budget>().FindAsync(budget.Id);
        saved!.IsDeleted.Should().BeTrue();
        saved.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_approved_budget_throws_AppException()
    {
        var budget = SeedBudget("Approved Budget", BudgetStatus.Approved);
        var handler = new DeleteBudgetCommandHandler(_context);

        Func<Task> act = () => handler.Handle(new DeleteBudgetCommand(budget.Id), default);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage("Only draft or cancelled budgets can be deleted.");
    }

    [Fact]
    public async Task Delete_active_budget_throws_AppException()
    {
        var budget = SeedBudget("Active Budget", BudgetStatus.Active);
        var handler = new DeleteBudgetCommandHandler(_context);

        Func<Task> act = () => handler.Handle(new DeleteBudgetCommand(budget.Id), default);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage("Only draft or cancelled budgets can be deleted.");
    }

    [Fact]
    public async Task Delete_throws_NotFoundException_for_missing_budget()
    {
        var handler = new DeleteBudgetCommandHandler(_context);
        var missingId = Guid.NewGuid();

        Func<Task> act = () => handler.Handle(new DeleteBudgetCommand(missingId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // -----------------------------------------------------------------------
    // GetBudgetsQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAll_excludes_deleted_budgets()
    {
        SeedBudget("Visible Budget", BudgetStatus.Draft, isDeleted: false);
        SeedBudget("Deleted Budget", BudgetStatus.Draft, isDeleted: true);

        var handler = new GetBudgetsQueryHandler(_context);
        var request = new PaginationRequest { Page = 1, PageSize = 20 };

        var result = await handler.Handle(new GetBudgetsQuery(request), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Name.Should().Be("Visible Budget");
    }

    [Fact]
    public async Task GetAll_filters_by_fiscal_year_id()
    {
        var targetFiscalYearId = Guid.NewGuid();
        SeedBudget("Target FY Budget", fiscalYearId: targetFiscalYearId);
        SeedBudget("Other FY Budget"); // different FiscalYearId

        var handler = new GetBudgetsQueryHandler(_context);
        var request = new PaginationRequest { Page = 1, PageSize = 20 };

        var result = await handler.Handle(
            new GetBudgetsQuery(request, FiscalYearId: targetFiscalYearId), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Name.Should().Be("Target FY Budget");
        result.Items.Single().FiscalYearId.Should().Be(targetFiscalYearId);
    }

    [Fact]
    public async Task GetAll_filters_by_name_search()
    {
        SeedBudget("Marketing Budget 2026");
        SeedBudget("Operations Budget 2026");
        SeedBudget("HR Budget 2026");

        var handler = new GetBudgetsQueryHandler(_context);
        var request = new PaginationRequest { Page = 1, PageSize = 20, Search = "Marketing" };

        var result = await handler.Handle(new GetBudgetsQuery(request), default);

        result.Items.Should().HaveCount(1);
        result.Items.Single().Name.Should().Be("Marketing Budget 2026");
    }

    [Fact]
    public async Task GetAll_sorts_ascending_by_name()
    {
        SeedBudget("Zebra Budget");
        SeedBudget("Alpha Budget");
        SeedBudget("Mango Budget");

        var handler = new GetBudgetsQueryHandler(_context);
        var request = new PaginationRequest { Page = 1, PageSize = 20, SortDescending = false };

        var result = await handler.Handle(new GetBudgetsQuery(request), default);

        result.Items.Select(i => i.Name).Should()
            .BeInAscendingOrder();
    }

    [Fact]
    public async Task GetAll_sorts_descending_by_name()
    {
        SeedBudget("Alpha Budget");
        SeedBudget("Mango Budget");
        SeedBudget("Zebra Budget");

        var handler = new GetBudgetsQueryHandler(_context);
        var request = new PaginationRequest { Page = 1, PageSize = 20, SortDescending = true };

        var result = await handler.Handle(new GetBudgetsQuery(request), default);

        result.Items.Select(i => i.Name).Should()
            .BeInDescendingOrder();
    }

    [Fact]
    public async Task GetAll_paginates_correctly()
    {
        // Seed 5 budgets with distinct, sortable names
        SeedBudget("Budget A");
        SeedBudget("Budget B");
        SeedBudget("Budget C");
        SeedBudget("Budget D");
        SeedBudget("Budget E");

        var handler = new GetBudgetsQueryHandler(_context);

        // Page 1: first 2 items (ascending by name)
        var requestPage1 = new PaginationRequest { Page = 1, PageSize = 2, SortDescending = false };
        var page1 = await handler.Handle(new GetBudgetsQuery(requestPage1), default);

        page1.TotalCount.Should().Be(5);
        page1.Items.Should().HaveCount(2);
        page1.Items[0].Name.Should().Be("Budget A");
        page1.Items[1].Name.Should().Be("Budget B");

        // Page 2: next 2 items
        var requestPage2 = new PaginationRequest { Page = 2, PageSize = 2, SortDescending = false };
        var page2 = await handler.Handle(new GetBudgetsQuery(requestPage2), default);

        page2.Items.Should().HaveCount(2);
        page2.Items[0].Name.Should().Be("Budget C");
        page2.Items[1].Name.Should().Be("Budget D");

        // Page 3: last item
        var requestPage3 = new PaginationRequest { Page = 3, PageSize = 2, SortDescending = false };
        var page3 = await handler.Handle(new GetBudgetsQuery(requestPage3), default);

        page3.Items.Should().HaveCount(1);
        page3.Items[0].Name.Should().Be("Budget E");
    }

    // -----------------------------------------------------------------------
    // GetBudgetByIdQueryHandler
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetById_returns_correct_budget()
    {
        var budget = SeedBudget("Specific Budget", BudgetStatus.Draft);
        // Seed a line so we can verify Lines are returned
        SeedBudgetLine(budget.Id, plannedAmount: 500m, period: 1);

        var handler = new GetBudgetByIdQueryHandler(_context);

        var result = await handler.Handle(new GetBudgetByIdQuery(budget.Id), default);

        result.Id.Should().Be(budget.Id);
        result.Name.Should().Be("Specific Budget");
        result.Status.Should().Be(BudgetStatus.Draft);
        result.Lines.Should().HaveCount(1);
        // Account nav is null (phantom FK) — mapper falls back to empty string
        result.Lines[0].AccountName.Should().Be(string.Empty);
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_missing_budget()
    {
        var handler = new GetBudgetByIdQueryHandler(_context);
        var missingId = Guid.NewGuid();

        Func<Task> act = () => handler.Handle(new GetBudgetByIdQuery(missingId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_deleted_budget()
    {
        var budget = SeedBudget("Deleted Budget", BudgetStatus.Draft, isDeleted: true);
        var handler = new GetBudgetByIdQueryHandler(_context);

        Func<Task> act = () => handler.Handle(new GetBudgetByIdQuery(budget.Id), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
