using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.Payment;
using uOrgHub.Accounts.Features.Payment;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Handlers;

public class PaymentHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public PaymentHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
    }

    public void Dispose() => _context.Dispose();

    // -------------------------------------------------------------------------
    // Seeding helpers
    // -------------------------------------------------------------------------

    private Customer SeedCustomer(string code = "C001")
    {
        var c = new Customer
        {
            Id = Guid.NewGuid(),
            CustomerCode = code,
            Name = "Test Customer",
            CreditLimit = 100000,
            PaymentTermsDays = 30,
            IsActive = true,
            ReceivableAccountId = Guid.NewGuid()
        };
        _context.Set<Customer>().Add(c);
        _context.SaveChanges();
        return c;
    }

    private Vendor SeedVendor(string code = "V001")
    {
        var v = new Vendor
        {
            Id = Guid.NewGuid(),
            VendorCode = code,
            Name = "Test Vendor",
            PaymentTermsDays = 30,
            IsActive = true,
            PayableAccountId = Guid.NewGuid()
        };
        _context.Set<Vendor>().Add(v);
        _context.SaveChanges();
        return v;
    }

    private Payment SeedPayment(
        string number,
        DateTime? date = null,
        bool isDeleted = false,
        Guid? customerId = null,
        Guid? vendorId = null)
    {
        var p = new Payment
        {
            Id = Guid.NewGuid(),
            PaymentNumber = number,
            PaymentType = PaymentType.CustomerPayment,
            PaymentMethod = PaymentMethod.Cash,
            PaymentDate = date ?? DateTime.Today,
            Amount = 1000,
            FiscalYearId = Guid.NewGuid(),
            CustomerId = customerId,
            VendorId = vendorId,
            IsDeleted = isDeleted
        };
        _context.Set<Payment>().Add(p);
        _context.SaveChanges();
        return p;
    }

    private Invoice SeedInvoice(decimal totalAmount)
    {
        var customer = SeedCustomer(Guid.NewGuid().ToString()[..8]);
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = $"INV-{Guid.NewGuid().ToString()[..6]}",
            CustomerId = customer.Id,
            FiscalYearId = Guid.NewGuid(),
            InvoiceDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Status = InvoiceStatus.Draft,
            TotalAmount = totalAmount,
            PaidAmount = 0
        };
        _context.Set<Invoice>().Add(invoice);
        _context.SaveChanges();
        return invoice;
    }

    private Bill SeedBill(decimal totalAmount)
    {
        var vendor = SeedVendor(Guid.NewGuid().ToString()[..8]);
        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            BillNumber = $"BILL-{Guid.NewGuid().ToString()[..6]}",
            VendorId = vendor.Id,
            FiscalYearId = Guid.NewGuid(),
            BillDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Status = BillStatus.Draft,
            TotalAmount = totalAmount,
            PaidAmount = 0
        };
        _context.Set<Bill>().Add(bill);
        _context.SaveChanges();
        return bill;
    }

    private CreatePaymentDto ValidCreateDto(string number = "PAY-001") => new()
    {
        PaymentNumber = number,
        PaymentType = PaymentType.CustomerPayment,
        PaymentMethod = PaymentMethod.Cash,
        PaymentDate = DateTime.Today,
        Amount = 1000,
        FiscalYearId = Guid.NewGuid(),
        Allocations = new List<CreatePaymentAllocationDto>()
    };

    // =========================================================================
    // CreatePaymentCommandHandler
    // =========================================================================

    [Fact]
    public async Task Create_saves_payment_with_correct_fields()
    {
        var handler = new CreatePaymentCommandHandler(_context);
        var dto = new CreatePaymentDto
        {
            PaymentNumber = "PAY-001",
            PaymentType = PaymentType.VendorPayment,
            PaymentMethod = PaymentMethod.BankTransfer,
            PaymentDate = new DateTime(2026, 5, 15),
            Amount = 2500.50m,
            ReferenceNumber = "REF-XYZ",
            FiscalYearId = Guid.NewGuid(),
            Allocations = new List<CreatePaymentAllocationDto>()
        };

        var result = await handler.Handle(new CreatePaymentCommand(dto), default);

        result.PaymentNumber.Should().Be("PAY-001");
        result.PaymentType.Should().Be(PaymentType.VendorPayment);
        result.PaymentMethod.Should().Be(PaymentMethod.BankTransfer);
        result.Amount.Should().Be(2500.50m);
        result.ReferenceNumber.Should().Be("REF-XYZ");
        _context.Set<Payment>().Count(p => !p.IsDeleted).Should().Be(1);
    }

    [Fact]
    public async Task Create_throws_when_payment_number_already_exists()
    {
        SeedPayment("PAY-DUP");
        var handler = new CreatePaymentCommandHandler(_context);

        var act = () => handler.Handle(new CreatePaymentCommand(ValidCreateDto("PAY-DUP")), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*PAY-DUP*");
    }

    [Fact]
    public async Task Create_allows_same_number_as_soft_deleted_payment()
    {
        SeedPayment("PAY-001", isDeleted: true);
        var handler = new CreatePaymentCommandHandler(_context);

        var result = await handler.Handle(new CreatePaymentCommand(ValidCreateDto("PAY-001")), default);

        result.PaymentNumber.Should().Be("PAY-001");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_throws_when_total_allocated_exceeds_payment_amount()
    {
        var handler = new CreatePaymentCommandHandler(_context);
        var dto = new CreatePaymentDto
        {
            PaymentNumber = "PAY-002",
            PaymentType = PaymentType.CustomerPayment,
            PaymentMethod = PaymentMethod.Cash,
            PaymentDate = DateTime.Today,
            Amount = 500,
            FiscalYearId = Guid.NewGuid(),
            Allocations = new List<CreatePaymentAllocationDto>
            {
                new() { AllocatedAmount = 350 },
                new() { AllocatedAmount = 260 }  // 350 + 260 = 610 > 500
            }
        };

        var act = () => handler.Handle(new CreatePaymentCommand(dto), default);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage("*Total allocated amount cannot exceed payment amount*");
    }

    [Fact]
    public async Task Create_with_invoice_allocation_updates_invoice_paid_amount()
    {
        var invoice = SeedInvoice(totalAmount: 1000);
        var handler = new CreatePaymentCommandHandler(_context);
        var dto = new CreatePaymentDto
        {
            PaymentNumber = "PAY-003",
            PaymentType = PaymentType.CustomerPayment,
            PaymentMethod = PaymentMethod.Cash,
            PaymentDate = DateTime.Today,
            Amount = 400,
            FiscalYearId = Guid.NewGuid(),
            Allocations = new List<CreatePaymentAllocationDto>
            {
                new() { InvoiceId = invoice.Id, AllocatedAmount = 400 }
            }
        };

        await handler.Handle(new CreatePaymentCommand(dto), default);

        var updatedInvoice = _context.Set<Invoice>().First(i => i.Id == invoice.Id);
        updatedInvoice.PaidAmount.Should().Be(400);
    }

    [Fact]
    public async Task Create_marks_invoice_as_paid_when_fully_paid()
    {
        var invoice = SeedInvoice(totalAmount: 800);
        var handler = new CreatePaymentCommandHandler(_context);
        var dto = new CreatePaymentDto
        {
            PaymentNumber = "PAY-004",
            PaymentType = PaymentType.CustomerPayment,
            PaymentMethod = PaymentMethod.Cash,
            PaymentDate = DateTime.Today,
            Amount = 800,
            FiscalYearId = Guid.NewGuid(),
            Allocations = new List<CreatePaymentAllocationDto>
            {
                new() { InvoiceId = invoice.Id, AllocatedAmount = 800 }
            }
        };

        await handler.Handle(new CreatePaymentCommand(dto), default);

        var updatedInvoice = _context.Set<Invoice>().First(i => i.Id == invoice.Id);
        updatedInvoice.Status.Should().Be(InvoiceStatus.Paid);
        updatedInvoice.PaidAmount.Should().Be(800);
    }

    [Fact]
    public async Task Create_marks_invoice_as_partially_paid_when_partially_allocated()
    {
        var invoice = SeedInvoice(totalAmount: 1200);
        var handler = new CreatePaymentCommandHandler(_context);
        var dto = new CreatePaymentDto
        {
            PaymentNumber = "PAY-005",
            PaymentType = PaymentType.CustomerPayment,
            PaymentMethod = PaymentMethod.Cash,
            PaymentDate = DateTime.Today,
            Amount = 500,
            FiscalYearId = Guid.NewGuid(),
            Allocations = new List<CreatePaymentAllocationDto>
            {
                new() { InvoiceId = invoice.Id, AllocatedAmount = 500 }
            }
        };

        await handler.Handle(new CreatePaymentCommand(dto), default);

        var updatedInvoice = _context.Set<Invoice>().First(i => i.Id == invoice.Id);
        updatedInvoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        updatedInvoice.PaidAmount.Should().Be(500);
    }

    [Fact]
    public async Task Create_with_bill_allocation_updates_bill_paid_amount_and_sets_partially_paid()
    {
        var bill = SeedBill(totalAmount: 2000);
        var handler = new CreatePaymentCommandHandler(_context);
        var dto = new CreatePaymentDto
        {
            PaymentNumber = "PAY-006",
            PaymentType = PaymentType.VendorPayment,
            PaymentMethod = PaymentMethod.BankTransfer,
            PaymentDate = DateTime.Today,
            Amount = 700,
            FiscalYearId = Guid.NewGuid(),
            Allocations = new List<CreatePaymentAllocationDto>
            {
                new() { BillId = bill.Id, AllocatedAmount = 700 }
            }
        };

        await handler.Handle(new CreatePaymentCommand(dto), default);

        var updatedBill = _context.Set<Bill>().First(b => b.Id == bill.Id);
        updatedBill.PaidAmount.Should().Be(700);
        updatedBill.Status.Should().Be(BillStatus.PartiallyPaid);
    }

    [Fact]
    public async Task Create_with_no_allocations_succeeds()
    {
        var handler = new CreatePaymentCommandHandler(_context);
        var dto = ValidCreateDto("PAY-007");

        var result = await handler.Handle(new CreatePaymentCommand(dto), default);

        result.Should().NotBeNull();
        result.PaymentNumber.Should().Be("PAY-007");
        result.Allocations.Should().BeEmpty();
    }

    // =========================================================================
    // GetPaymentsQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetAll_excludes_deleted_payments()
    {
        SeedPayment("PAY-A");
        SeedPayment("PAY-B");
        SeedPayment("PAY-DEL", isDeleted: true);
        var handler = new GetPaymentsQueryHandler(_context);

        var result = await handler.Handle(
            new GetPaymentsQuery(new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().NotContain(p => p.PaymentNumber == "PAY-DEL");
    }

    [Fact]
    public async Task GetAll_filters_by_customer_id()
    {
        var customer1 = SeedCustomer("C001");
        var customer2 = SeedCustomer("C002");
        SeedPayment("PAY-C1-1", customerId: customer1.Id);
        SeedPayment("PAY-C1-2", customerId: customer1.Id);
        SeedPayment("PAY-C2-1", customerId: customer2.Id);
        var handler = new GetPaymentsQueryHandler(_context);

        var result = await handler.Handle(
            new GetPaymentsQuery(new PaginationRequest { Page = 1, PageSize = 10 }, CustomerId: customer1.Id),
            default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(p => p.CustomerId.Should().Be(customer1.Id));
    }

    [Fact]
    public async Task GetAll_filters_by_vendor_id()
    {
        var vendor1 = SeedVendor("V001");
        var vendor2 = SeedVendor("V002");
        SeedPayment("PAY-V1-1", vendorId: vendor1.Id);
        SeedPayment("PAY-V2-1", vendorId: vendor2.Id);
        SeedPayment("PAY-V2-2", vendorId: vendor2.Id);
        var handler = new GetPaymentsQueryHandler(_context);

        var result = await handler.Handle(
            new GetPaymentsQuery(new PaginationRequest { Page = 1, PageSize = 10 }, VendorId: vendor2.Id),
            default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(p => p.VendorId.Should().Be(vendor2.Id));
    }

    [Fact]
    public async Task GetAll_filters_by_payment_number_search()
    {
        SeedPayment("CUST-2026-001");
        SeedPayment("CUST-2026-002");
        SeedPayment("VEND-999");
        var handler = new GetPaymentsQueryHandler(_context);

        var result = await handler.Handle(
            new GetPaymentsQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "CUST-2026" }),
            default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(p => p.PaymentNumber.Should().Contain("CUST-2026"));
    }

    [Fact]
    public async Task GetAll_sorts_by_payment_date_ascending()
    {
        var dateOldest = new DateTime(2026, 1, 1);
        var dateMiddle = new DateTime(2026, 3, 15);
        var dateNewest = new DateTime(2026, 5, 10);

        SeedPayment("PAY-MID", date: dateMiddle);
        SeedPayment("PAY-NEW", date: dateNewest);
        SeedPayment("PAY-OLD", date: dateOldest);

        var handler = new GetPaymentsQueryHandler(_context);

        var result = await handler.Handle(
            new GetPaymentsQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = false }),
            default);

        result.Items[0].PaymentNumber.Should().Be("PAY-OLD");
        result.Items[1].PaymentNumber.Should().Be("PAY-MID");
        result.Items[2].PaymentNumber.Should().Be("PAY-NEW");
    }

    [Fact]
    public async Task GetAll_paginates_correctly()
    {
        for (int i = 1; i <= 7; i++)
            SeedPayment($"PAY-{i:D3}", date: new DateTime(2026, 1, i));

        var handler = new GetPaymentsQueryHandler(_context);

        var page1 = await handler.Handle(
            new GetPaymentsQuery(new PaginationRequest { Page = 1, PageSize = 3 }), default);
        var page2 = await handler.Handle(
            new GetPaymentsQuery(new PaginationRequest { Page = 2, PageSize = 3 }), default);
        var page3 = await handler.Handle(
            new GetPaymentsQuery(new PaginationRequest { Page = 3, PageSize = 3 }), default);

        page1.TotalCount.Should().Be(7);
        page1.Items.Should().HaveCount(3);
        page2.Items.Should().HaveCount(3);
        page3.Items.Should().HaveCount(1);
    }

    // =========================================================================
    // GetPaymentByIdQueryHandler
    // =========================================================================

    [Fact]
    public async Task GetById_returns_correct_payment_with_allocations()
    {
        var payment = SeedPayment("PAY-XYZ");

        // Seed an allocation directly so we can verify it comes back
        var allocation = new PaymentAllocation
        {
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            AllocatedAmount = 250
        };
        _context.Set<PaymentAllocation>().Add(allocation);
        _context.SaveChanges();

        var handler = new GetPaymentByIdQueryHandler(_context);

        var result = await handler.Handle(new GetPaymentByIdQuery(payment.Id), default);

        result.Id.Should().Be(payment.Id);
        result.PaymentNumber.Should().Be("PAY-XYZ");
        result.Allocations.Should().HaveCount(1);
        result.Allocations[0].AllocatedAmount.Should().Be(250);
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_missing_payment()
    {
        var handler = new GetPaymentByIdQueryHandler(_context);

        var act = () => handler.Handle(new GetPaymentByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_deleted_payment()
    {
        var payment = SeedPayment("PAY-DEL2", isDeleted: true);
        var handler = new GetPaymentByIdQueryHandler(_context);

        var act = () => handler.Handle(new GetPaymentByIdQuery(payment.Id), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
