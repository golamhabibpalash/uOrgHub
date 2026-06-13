using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using uOrgHub.Accounts.DTOs.AP;
using uOrgHub.Accounts.Features.AP;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Services;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Handlers;

public class BillHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public BillHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
    }

    public void Dispose() => _context.Dispose();

    private Vendor SeedVendor(string code = "V001")
    {
        var v = new Vendor
        {
            Id = Guid.NewGuid(), VendorCode = code, Name = "Test Vendor",
            PaymentTermsDays = 30, IsActive = true, PayableAccountId = Guid.NewGuid()
        };
        _context.Set<Vendor>().Add(v);
        _context.SaveChanges();
        return v;
    }

    private Bill SeedBill(Vendor vendor, string billNumber, BillStatus status = BillStatus.Draft, bool isDeleted = false)
    {
        var bill = new Bill
        {
            Id = Guid.NewGuid(),
            BillNumber = billNumber,
            VendorId = vendor.Id,
            FiscalYearId = Guid.NewGuid(),
            BillDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Status = status,
            IsDeleted = isDeleted
        };
        _context.Set<Bill>().Add(bill);
        _context.SaveChanges();
        return bill;
    }

    private CreateBillCommandHandler CreateHandler()
    {
        var numbering = new Mock<IDocumentNumberingService>();
        numbering.Setup(x => x.GenerateNextAsync("Bill", "BL", null, null))
            .ReturnsAsync("BL-2506-000001");
        return new CreateBillCommandHandler(_context, numbering.Object);
    }

    private CreateBillDto ValidCreateDto(Guid vendorId, string billNumber = "BILL-001") => new()
    {
        BillNumber = billNumber,
        VendorId = vendorId,
        FiscalYearId = Guid.NewGuid(),
        BillDate = DateTime.Today,
        DueDate = DateTime.Today.AddDays(30),
        Lines = new List<CreateBillLineDto>
        {
            new() { Description = "Office supplies", Quantity = 5, UnitPrice = 100, DiscountPercent = 0, ExpenseAccountId = Guid.NewGuid() }
        }
    };

    // --- CreateBillCommandHandler ---

    [Fact]
    public async Task Create_saves_bill_with_draft_status()
    {
        var vendor = SeedVendor();
        var handler = CreateHandler();

        var result = await handler.Handle(new CreateBillCommand(ValidCreateDto(vendor.Id)), default);

        result.BillNumber.Should().Be("BILL-001");
        result.Status.Should().Be(BillStatus.Draft);
        _context.Set<Bill>().Count(b => !b.IsDeleted).Should().Be(1);
    }

    [Fact]
    public async Task Create_calculates_subtotal_correctly()
    {
        var vendor = SeedVendor();
        var dto = ValidCreateDto(vendor.Id);
        dto.Lines = new List<CreateBillLineDto>
        {
            new() { Description = "Item A", Quantity = 10, UnitPrice = 50, DiscountPercent = 0, ExpenseAccountId = Guid.NewGuid() },
            new() { Description = "Item B", Quantity = 2, UnitPrice = 200, DiscountPercent = 0, ExpenseAccountId = Guid.NewGuid() }
        };
        var handler = CreateHandler();

        var result = await handler.Handle(new CreateBillCommand(dto), default);

        result.SubTotal.Should().Be(900); // (10×50) + (2×200)
    }

    [Fact]
    public async Task Create_applies_discount_to_line_total()
    {
        var vendor = SeedVendor();
        var dto = ValidCreateDto(vendor.Id);
        dto.Lines = new List<CreateBillLineDto>
        {
            new() { Description = "Item", Quantity = 10, UnitPrice = 100, DiscountPercent = 10, ExpenseAccountId = Guid.NewGuid() }
        };
        var handler = CreateHandler();

        var result = await handler.Handle(new CreateBillCommand(dto), default);

        result.SubTotal.Should().Be(900); // 10 × 100 × 0.90
    }

    [Fact]
    public async Task Create_throws_when_bill_number_already_exists()
    {
        var vendor = SeedVendor();
        SeedBill(vendor, "BILL-001");
        var handler = CreateHandler();

        var act = () => handler.Handle(new CreateBillCommand(ValidCreateDto(vendor.Id, "BILL-001")), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*BILL-001*");
    }

    [Fact]
    public async Task Create_allows_same_number_as_soft_deleted_bill()
    {
        var vendor = SeedVendor();
        SeedBill(vendor, "BILL-001", isDeleted: true);
        var handler = CreateHandler();

        var result = await handler.Handle(new CreateBillCommand(ValidCreateDto(vendor.Id, "BILL-001")), default);
        result.BillNumber.Should().Be("BILL-001");
    }

    // --- UpdateBillCommandHandler ---

    [Fact]
    public async Task Update_draft_bill_succeeds()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-002", BillStatus.Draft);
        var dto = new UpdateBillDto
        {
            BillDate = DateTime.Today.AddDays(-1),
            DueDate = DateTime.Today.AddDays(60),
            Lines = new List<CreateBillLineDto>()
        };
        var handler = new UpdateBillCommandHandler(_context);

        var result = await handler.Handle(new UpdateBillCommand(bill.Id, dto), default);

        result.BillDate.Should().Be(DateTime.Today.AddDays(-1));
        result.DueDate.Should().Be(DateTime.Today.AddDays(60));
    }

    [Fact]
    public async Task Update_non_draft_bill_throws_AppException()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-003", BillStatus.Received);
        var dto = new UpdateBillDto
        {
            BillDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            Lines = new List<CreateBillLineDto>
            {
                new() { Description = "X", Quantity = 1, UnitPrice = 100, ExpenseAccountId = Guid.NewGuid() }
            }
        };
        var handler = new UpdateBillCommandHandler(_context);

        var act = () => handler.Handle(new UpdateBillCommand(bill.Id, dto), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*draft*");
    }

    [Fact]
    public async Task Update_throws_NotFoundException_for_missing_bill()
    {
        var handler = new UpdateBillCommandHandler(_context);
        var act = () => handler.Handle(new UpdateBillCommand(Guid.NewGuid(), new UpdateBillDto
        {
            BillDate = DateTime.Today, DueDate = DateTime.Today.AddDays(1),
            Lines = new List<CreateBillLineDto> { new() { Description = "X", Quantity = 1, UnitPrice = 1, ExpenseAccountId = Guid.NewGuid() } }
        }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- ApproveBillCommandHandler ---

    [Fact]
    public async Task Approve_changes_status_to_Received()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-004", BillStatus.Draft);
        var handler = new ApproveBillCommandHandler(_context);

        var result = await handler.Handle(new ApproveBillCommand(bill.Id), default);

        result.Status.Should().Be(BillStatus.Received);
    }

    [Fact]
    public async Task Approve_non_draft_bill_throws_AppException()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-005", BillStatus.Received);
        var handler = new ApproveBillCommandHandler(_context);

        var act = () => handler.Handle(new ApproveBillCommand(bill.Id), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*draft*");
    }

    [Fact]
    public async Task Approve_throws_NotFoundException_for_missing_bill()
    {
        var handler = new ApproveBillCommandHandler(_context);
        var act = () => handler.Handle(new ApproveBillCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- VoidBillCommandHandler ---

    [Fact]
    public async Task Void_draft_bill_sets_status_to_Void()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-006", BillStatus.Draft);
        var handler = new VoidBillCommandHandler(_context);

        var result = await handler.Handle(new VoidBillCommand(bill.Id), default);

        result.Status.Should().Be(BillStatus.Void);
    }

    [Fact]
    public async Task Void_received_bill_sets_status_to_Void()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-007", BillStatus.Received);
        var handler = new VoidBillCommandHandler(_context);

        var result = await handler.Handle(new VoidBillCommand(bill.Id), default);

        result.Status.Should().Be(BillStatus.Void);
    }

    [Fact]
    public async Task Void_paid_bill_throws_AppException()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-008", BillStatus.Paid);
        var handler = new VoidBillCommandHandler(_context);

        var act = () => handler.Handle(new VoidBillCommand(bill.Id), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*Paid*");
    }

    [Fact]
    public async Task Void_already_void_bill_throws_AppException()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-009", BillStatus.Void);
        var handler = new VoidBillCommandHandler(_context);

        var act = () => handler.Handle(new VoidBillCommand(bill.Id), default);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Void_throws_NotFoundException_for_missing_bill()
    {
        var handler = new VoidBillCommandHandler(_context);
        var act = () => handler.Handle(new VoidBillCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- GetBillsQueryHandler ---

    [Fact]
    public async Task GetAll_excludes_deleted_bills()
    {
        var vendor = SeedVendor();
        SeedBill(vendor, "BILL-A");
        SeedBill(vendor, "BILL-B");
        SeedBill(vendor, "BILL-DEL", isDeleted: true);
        var handler = new GetBillsQueryHandler(_context);

        var result = await handler.Handle(new GetBillsQuery(new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAll_filters_by_vendor_id()
    {
        var vendor1 = SeedVendor("V001");
        var vendor2 = SeedVendor("V002");
        SeedBill(vendor1, "BILL-V1-1");
        SeedBill(vendor1, "BILL-V1-2");
        SeedBill(vendor2, "BILL-V2-1");
        var handler = new GetBillsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBillsQuery(new PaginationRequest { Page = 1, PageSize = 10 }, VendorId: vendor1.Id), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(b => b.VendorId.Should().Be(vendor1.Id));
    }

    [Fact]
    public async Task GetAll_filters_by_status()
    {
        var vendor = SeedVendor();
        SeedBill(vendor, "BILL-D1", BillStatus.Draft);
        SeedBill(vendor, "BILL-D2", BillStatus.Draft);
        SeedBill(vendor, "BILL-R1", BillStatus.Received);
        var handler = new GetBillsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBillsQuery(new PaginationRequest { Page = 1, PageSize = 10 }, Status: BillStatus.Draft), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(b => b.Status.Should().Be(BillStatus.Draft));
    }

    [Fact]
    public async Task GetAll_filters_by_bill_number_search()
    {
        var vendor = SeedVendor();
        SeedBill(vendor, "INV-2026-001");
        SeedBill(vendor, "INV-2026-002");
        SeedBill(vendor, "BILL-999");
        var handler = new GetBillsQueryHandler(_context);

        var result = await handler.Handle(
            new GetBillsQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "INV-2026" }), default);

        result.TotalCount.Should().Be(2);
    }

    // --- GetBillByIdQueryHandler ---

    [Fact]
    public async Task GetById_returns_bill_with_lines()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-X1");
        var handler = new GetBillByIdQueryHandler(_context);

        var result = await handler.Handle(new GetBillByIdQuery(bill.Id), default);

        result.Id.Should().Be(bill.Id);
        result.BillNumber.Should().Be("BILL-X1");
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_missing_bill()
    {
        var handler = new GetBillByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetBillByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_deleted_bill()
    {
        var vendor = SeedVendor();
        var bill = SeedBill(vendor, "BILL-DEL2", isDeleted: true);
        var handler = new GetBillByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetBillByIdQuery(bill.Id), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
