using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.Features.AR;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Handlers;

public class CustomerHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public CustomerHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
    }

    public void Dispose() => _context.Dispose();

    private Customer SeedCustomer(string code, string name, bool isDeleted = false)
    {
        var c = new Customer
        {
            Id = Guid.NewGuid(), CustomerCode = code, Name = name,
            CreditLimit = 100000, PaymentTermsDays = 30, IsActive = true,
            IsDeleted = isDeleted, ReceivableAccountId = Guid.NewGuid()
        };
        _context.Set<Customer>().Add(c);
        _context.SaveChanges();
        return c;
    }

    private CreateCustomerDto ValidCreateDto(string code = "C001") => new()
    {
        CustomerCode = code, Name = "TechCorp Ltd",
        CreditLimit = 500000, PaymentTermsDays = 30,
        ReceivableAccountId = Guid.NewGuid()
    };

    // --- CreateCustomerCommandHandler ---

    [Fact]
    public async Task Create_saves_customer_and_returns_dto()
    {
        var handler = new CreateCustomerCommandHandler(_context);
        var result = await handler.Handle(new CreateCustomerCommand(ValidCreateDto("C001")), default);

        result.CustomerCode.Should().Be("C001");
        result.Name.Should().Be("TechCorp Ltd");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_throws_when_customer_code_already_exists()
    {
        SeedCustomer("C001", "Existing");
        var handler = new CreateCustomerCommandHandler(_context);

        var act = () => handler.Handle(new CreateCustomerCommand(ValidCreateDto("C001")), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*C001*");
    }

    [Fact]
    public async Task Create_allows_same_code_as_soft_deleted_customer()
    {
        SeedCustomer("C001", "Old", isDeleted: true);
        var handler = new CreateCustomerCommandHandler(_context);

        var result = await handler.Handle(new CreateCustomerCommand(ValidCreateDto("C001")), default);
        result.CustomerCode.Should().Be("C001");
    }

    // --- UpdateCustomerCommandHandler ---

    [Fact]
    public async Task Update_modifies_customer_fields()
    {
        var customer = SeedCustomer("C002", "Old Name");
        var dto = new UpdateCustomerDto { Name = "New Name", CreditLimit = 200000, PaymentTermsDays = 45, IsActive = true };
        var handler = new UpdateCustomerCommandHandler(_context);

        var result = await handler.Handle(new UpdateCustomerCommand(customer.Id, dto), default);

        result.Name.Should().Be("New Name");
        result.CreditLimit.Should().Be(200000);
        result.PaymentTermsDays.Should().Be(45);
    }

    [Fact]
    public async Task Update_throws_NotFoundException_when_not_found()
    {
        var handler = new UpdateCustomerCommandHandler(_context);
        var act = () => handler.Handle(new UpdateCustomerCommand(Guid.NewGuid(), new UpdateCustomerDto { Name = "X" }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_throws_NotFoundException_for_soft_deleted_customer()
    {
        var c = SeedCustomer("C003", "Deleted", isDeleted: true);
        var handler = new UpdateCustomerCommandHandler(_context);
        var act = () => handler.Handle(new UpdateCustomerCommand(c.Id, new UpdateCustomerDto { Name = "X" }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- DeleteCustomerCommandHandler ---

    [Fact]
    public async Task Delete_soft_deletes_customer()
    {
        var customer = SeedCustomer("C004", "To Delete");
        var handler = new DeleteCustomerCommandHandler(_context);

        var result = await handler.Handle(new DeleteCustomerCommand(customer.Id), default);

        result.Should().Be(Unit.Value);
        _context.Set<Customer>().First(c => c.Id == customer.Id).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_throws_NotFoundException_when_not_found()
    {
        var handler = new DeleteCustomerCommandHandler(_context);
        var act = () => handler.Handle(new DeleteCustomerCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- GetCustomersQueryHandler ---

    [Fact]
    public async Task GetAll_excludes_deleted_customers()
    {
        SeedCustomer("C001", "Alpha");
        SeedCustomer("C002", "Beta");
        SeedCustomer("C003", "Deleted", isDeleted: true);
        var handler = new GetCustomersQueryHandler(_context);

        var result = await handler.Handle(new GetCustomersQuery(new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAll_filters_by_name_search()
    {
        SeedCustomer("C001", "Alpha Corp");
        SeedCustomer("C002", "Beta Solutions");
        var handler = new GetCustomersQueryHandler(_context);

        var result = await handler.Handle(
            new GetCustomersQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "Alpha" }), default);

        result.TotalCount.Should().Be(1);
        result.Items[0].Name.Should().Be("Alpha Corp");
    }

    [Fact]
    public async Task GetAll_filters_by_customer_code()
    {
        SeedCustomer("CUST-100", "One");
        SeedCustomer("CUST-200", "Two");
        var handler = new GetCustomersQueryHandler(_context);

        var result = await handler.Handle(
            new GetCustomersQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "CUST-200" }), default);

        result.TotalCount.Should().Be(1);
        result.Items[0].CustomerCode.Should().Be("CUST-200");
    }

    [Fact]
    public async Task GetAll_sorts_ascending_by_name()
    {
        SeedCustomer("C002", "Zeta");
        SeedCustomer("C001", "Alpha");
        var handler = new GetCustomersQueryHandler(_context);

        var result = await handler.Handle(
            new GetCustomersQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = false }), default);

        result.Items[0].Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task GetAll_sorts_descending_by_name()
    {
        SeedCustomer("C002", "Zeta");
        SeedCustomer("C001", "Alpha");
        var handler = new GetCustomersQueryHandler(_context);

        var result = await handler.Handle(
            new GetCustomersQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = true }), default);

        result.Items[0].Name.Should().Be("Zeta");
    }

    [Fact]
    public async Task GetAll_paginates_correctly()
    {
        for (int i = 1; i <= 7; i++) SeedCustomer($"C{i:D3}", $"Customer {i:D2}");
        var handler = new GetCustomersQueryHandler(_context);

        var page1 = await handler.Handle(new GetCustomersQuery(new PaginationRequest { Page = 1, PageSize = 5 }), default);
        var page2 = await handler.Handle(new GetCustomersQuery(new PaginationRequest { Page = 2, PageSize = 5 }), default);

        page1.Items.Should().HaveCount(5);
        page2.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(7);
    }

    // --- GetCustomerByIdQueryHandler ---

    [Fact]
    public async Task GetById_returns_correct_customer()
    {
        var customer = SeedCustomer("C010", "Target");
        var handler = new GetCustomerByIdQueryHandler(_context);

        var result = await handler.Handle(new GetCustomerByIdQuery(customer.Id), default);

        result.Id.Should().Be(customer.Id);
        result.CustomerCode.Should().Be("C010");
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_missing_customer()
    {
        var handler = new GetCustomerByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetCustomerByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_deleted_customer()
    {
        var customer = SeedCustomer("C011", "Deleted", isDeleted: true);
        var handler = new GetCustomerByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetCustomerByIdQuery(customer.Id), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
