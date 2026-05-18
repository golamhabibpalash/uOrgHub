using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.Features.AR;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Handlers;

public class InvoiceHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public InvoiceHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
    }

    public void Dispose() => _context.Dispose();

    private Customer SeedCustomer(string code = "C001")
    {
        var c = new Customer
        {
            Id = Guid.NewGuid(), CustomerCode = code, Name = "Test Customer",
            CreditLimit = 100000, PaymentTermsDays = 30, IsActive = true,
            ReceivableAccountId = Guid.NewGuid()
        };
        _context.Set<Customer>().Add(c);
        _context.SaveChanges();
        return c;
    }

    private Invoice SeedInvoice(Customer customer, string invoiceNumber, InvoiceStatus status = InvoiceStatus.Draft, bool isDeleted = false)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            CustomerId = customer.Id,
            FiscalYearId = Guid.NewGuid(),
            InvoiceDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Status = status,
            IsDeleted = isDeleted
        };
        _context.Set<Invoice>().Add(invoice);
        _context.SaveChanges();
        return invoice;
    }

    private CreateInvoiceDto ValidCreateDto(Guid customerId, string invoiceNumber = "INV-001") => new()
    {
        InvoiceNumber = invoiceNumber,
        CustomerId = customerId,
        FiscalYearId = Guid.NewGuid(),
        InvoiceDate = DateTime.Today,
        DueDate = DateTime.Today.AddDays(30),
        Lines = new List<CreateInvoiceLineDto>
        {
            new() { Description = "Consulting", Quantity = 5, UnitPrice = 200, DiscountPercent = 0, RevenueAccountId = Guid.NewGuid() }
        }
    };

    // --- CreateInvoiceCommandHandler ---

    [Fact]
    public async Task Create_saves_invoice_with_draft_status()
    {
        var customer = SeedCustomer();
        var handler = new CreateInvoiceCommandHandler(_context);

        var result = await handler.Handle(new CreateInvoiceCommand(ValidCreateDto(customer.Id)), default);

        result.InvoiceNumber.Should().Be("INV-001");
        result.Status.Should().Be(InvoiceStatus.Draft);
        _context.Set<Invoice>().Count(i => !i.IsDeleted).Should().Be(1);
    }

    [Fact]
    public async Task Create_calculates_subtotal_correctly()
    {
        var customer = SeedCustomer();
        var dto = ValidCreateDto(customer.Id);
        dto.Lines = new List<CreateInvoiceLineDto>
        {
            new() { Description = "Service A", Quantity = 4, UnitPrice = 250, DiscountPercent = 0, RevenueAccountId = Guid.NewGuid() },
            new() { Description = "Service B", Quantity = 2, UnitPrice = 300, DiscountPercent = 0, RevenueAccountId = Guid.NewGuid() }
        };
        var handler = new CreateInvoiceCommandHandler(_context);

        var result = await handler.Handle(new CreateInvoiceCommand(dto), default);

        result.SubTotal.Should().Be(1600); // (4×250) + (2×300)
    }

    [Fact]
    public async Task Create_applies_discount_to_line_total()
    {
        var customer = SeedCustomer();
        var dto = ValidCreateDto(customer.Id);
        dto.Lines = new List<CreateInvoiceLineDto>
        {
            new() { Description = "Item", Quantity = 10, UnitPrice = 100, DiscountPercent = 20, RevenueAccountId = Guid.NewGuid() }
        };
        var handler = new CreateInvoiceCommandHandler(_context);

        var result = await handler.Handle(new CreateInvoiceCommand(dto), default);

        result.SubTotal.Should().Be(800); // 10 × 100 × 0.80
    }

    [Fact]
    public async Task Create_throws_when_invoice_number_already_exists()
    {
        var customer = SeedCustomer();
        SeedInvoice(customer, "INV-001");
        var handler = new CreateInvoiceCommandHandler(_context);

        var act = () => handler.Handle(new CreateInvoiceCommand(ValidCreateDto(customer.Id, "INV-001")), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*INV-001*");
    }

    [Fact]
    public async Task Create_allows_same_number_as_soft_deleted_invoice()
    {
        var customer = SeedCustomer();
        SeedInvoice(customer, "INV-001", isDeleted: true);
        var handler = new CreateInvoiceCommandHandler(_context);

        var result = await handler.Handle(new CreateInvoiceCommand(ValidCreateDto(customer.Id, "INV-001")), default);
        result.InvoiceNumber.Should().Be("INV-001");
    }

    // --- UpdateInvoiceCommandHandler ---

    [Fact]
    public async Task Update_draft_invoice_succeeds()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-002", InvoiceStatus.Draft);
        var dto = new UpdateInvoiceDto
        {
            InvoiceDate = DateTime.Today.AddDays(-1),
            DueDate = DateTime.Today.AddDays(60),
            Lines = new List<CreateInvoiceLineDto>()
        };
        var handler = new UpdateInvoiceCommandHandler(_context);

        var result = await handler.Handle(new UpdateInvoiceCommand(invoice.Id, dto), default);

        result.InvoiceDate.Should().Be(DateTime.Today.AddDays(-1));
        result.DueDate.Should().Be(DateTime.Today.AddDays(60));
    }

    [Fact]
    public async Task Update_non_draft_invoice_throws_AppException()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-003", InvoiceStatus.Sent);
        var dto = new UpdateInvoiceDto
        {
            InvoiceDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Lines = new List<CreateInvoiceLineDto>
            {
                new() { Description = "X", Quantity = 1, UnitPrice = 100, RevenueAccountId = Guid.NewGuid() }
            }
        };
        var handler = new UpdateInvoiceCommandHandler(_context);

        var act = () => handler.Handle(new UpdateInvoiceCommand(invoice.Id, dto), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*draft*");
    }

    [Fact]
    public async Task Update_throws_NotFoundException_for_missing_invoice()
    {
        var handler = new UpdateInvoiceCommandHandler(_context);
        var act = () => handler.Handle(new UpdateInvoiceCommand(Guid.NewGuid(), new UpdateInvoiceDto
        {
            InvoiceDate = DateTime.Today, DueDate = DateTime.Today.AddDays(1),
            Lines = new List<CreateInvoiceLineDto> { new() { Description = "X", Quantity = 1, UnitPrice = 1, RevenueAccountId = Guid.NewGuid() } }
        }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- PostInvoiceCommandHandler ---

    [Fact]
    public async Task Post_changes_status_to_Sent()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-004", InvoiceStatus.Draft);
        var handler = new PostInvoiceCommandHandler(_context);

        var result = await handler.Handle(new PostInvoiceCommand(invoice.Id), default);

        result.Status.Should().Be(InvoiceStatus.Sent);
    }

    [Fact]
    public async Task Post_non_draft_invoice_throws_AppException()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-005", InvoiceStatus.Sent);
        var handler = new PostInvoiceCommandHandler(_context);

        var act = () => handler.Handle(new PostInvoiceCommand(invoice.Id), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*draft*");
    }

    [Fact]
    public async Task Post_throws_NotFoundException_for_missing_invoice()
    {
        var handler = new PostInvoiceCommandHandler(_context);
        var act = () => handler.Handle(new PostInvoiceCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- VoidInvoiceCommandHandler ---

    [Fact]
    public async Task Void_draft_invoice_sets_status_to_Void()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-006", InvoiceStatus.Draft);
        var handler = new VoidInvoiceCommandHandler(_context);

        var result = await handler.Handle(new VoidInvoiceCommand(invoice.Id), default);

        result.Status.Should().Be(InvoiceStatus.Void);
    }

    [Fact]
    public async Task Void_sent_invoice_sets_status_to_Void()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-007", InvoiceStatus.Sent);
        var handler = new VoidInvoiceCommandHandler(_context);

        var result = await handler.Handle(new VoidInvoiceCommand(invoice.Id), default);

        result.Status.Should().Be(InvoiceStatus.Void);
    }

    [Fact]
    public async Task Void_paid_invoice_throws_AppException()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-008", InvoiceStatus.Paid);
        var handler = new VoidInvoiceCommandHandler(_context);

        var act = () => handler.Handle(new VoidInvoiceCommand(invoice.Id), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*Paid*");
    }

    [Fact]
    public async Task Void_already_void_invoice_throws_AppException()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-009", InvoiceStatus.Void);
        var handler = new VoidInvoiceCommandHandler(_context);

        var act = () => handler.Handle(new VoidInvoiceCommand(invoice.Id), default);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Void_throws_NotFoundException_for_missing_invoice()
    {
        var handler = new VoidInvoiceCommandHandler(_context);
        var act = () => handler.Handle(new VoidInvoiceCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- GetInvoicesQueryHandler ---

    [Fact]
    public async Task GetAll_excludes_deleted_invoices()
    {
        var customer = SeedCustomer();
        SeedInvoice(customer, "INV-A");
        SeedInvoice(customer, "INV-B");
        SeedInvoice(customer, "INV-DEL", isDeleted: true);
        var handler = new GetInvoicesQueryHandler(_context);

        var result = await handler.Handle(new GetInvoicesQuery(new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAll_filters_by_customer_id()
    {
        var customer1 = SeedCustomer("C001");
        var customer2 = SeedCustomer("C002");
        SeedInvoice(customer1, "INV-C1-1");
        SeedInvoice(customer1, "INV-C1-2");
        SeedInvoice(customer2, "INV-C2-1");
        var handler = new GetInvoicesQueryHandler(_context);

        var result = await handler.Handle(
            new GetInvoicesQuery(new PaginationRequest { Page = 1, PageSize = 10 }, CustomerId: customer1.Id), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(i => i.CustomerId.Should().Be(customer1.Id));
    }

    [Fact]
    public async Task GetAll_filters_by_status()
    {
        var customer = SeedCustomer();
        SeedInvoice(customer, "INV-D1", InvoiceStatus.Draft);
        SeedInvoice(customer, "INV-D2", InvoiceStatus.Draft);
        SeedInvoice(customer, "INV-S1", InvoiceStatus.Sent);
        var handler = new GetInvoicesQueryHandler(_context);

        var result = await handler.Handle(
            new GetInvoicesQuery(new PaginationRequest { Page = 1, PageSize = 10 }, Status: InvoiceStatus.Draft), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(i => i.Status.Should().Be(InvoiceStatus.Draft));
    }

    [Fact]
    public async Task GetAll_filters_by_invoice_number_search()
    {
        var customer = SeedCustomer();
        SeedInvoice(customer, "SALES-2026-001");
        SeedInvoice(customer, "SALES-2026-002");
        SeedInvoice(customer, "MISC-999");
        var handler = new GetInvoicesQueryHandler(_context);

        var result = await handler.Handle(
            new GetInvoicesQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "SALES-2026" }), default);

        result.TotalCount.Should().Be(2);
    }

    // --- GetInvoiceByIdQueryHandler ---

    [Fact]
    public async Task GetById_returns_invoice_with_lines()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-X1");
        var handler = new GetInvoiceByIdQueryHandler(_context);

        var result = await handler.Handle(new GetInvoiceByIdQuery(invoice.Id), default);

        result.Id.Should().Be(invoice.Id);
        result.InvoiceNumber.Should().Be("INV-X1");
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_missing_invoice()
    {
        var handler = new GetInvoiceByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetInvoiceByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_deleted_invoice()
    {
        var customer = SeedCustomer();
        var invoice = SeedInvoice(customer, "INV-DEL2", isDeleted: true);
        var handler = new GetInvoiceByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetInvoiceByIdQuery(invoice.Id), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
