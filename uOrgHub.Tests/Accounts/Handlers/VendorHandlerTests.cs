using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.AP;
using uOrgHub.Accounts.Features.AP;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Handlers;

public class VendorHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public VendorHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
    }

    public void Dispose() => _context.Dispose();

    private Vendor SeedVendor(string code, string name, bool isDeleted = false)
    {
        var v = new Vendor
        {
            Id = Guid.NewGuid(), VendorCode = code, Name = name,
            PaymentTermsDays = 30, IsActive = true, IsDeleted = isDeleted,
            PayableAccountId = Guid.NewGuid()
        };
        _context.Set<Vendor>().Add(v);
        _context.SaveChanges();
        return v;
    }

    private CreateVendorDto ValidCreateDto(string code = "V001") => new()
    {
        VendorCode = code, Name = "Acme Supplies", PaymentTermsDays = 30,
        PayableAccountId = Guid.NewGuid()
    };

    // --- CreateVendorCommandHandler ---

    [Fact]
    public async Task Create_saves_vendor_and_returns_dto()
    {
        var handler = new CreateVendorCommandHandler(_context);
        var result = await handler.Handle(new CreateVendorCommand(ValidCreateDto("V001")), default);

        result.Should().NotBeNull();
        result.VendorCode.Should().Be("V001");
        result.Name.Should().Be("Acme Supplies");
        _context.Set<Vendor>().Count(v => !v.IsDeleted).Should().Be(1);
    }

    [Fact]
    public async Task Create_throws_when_vendor_code_already_exists()
    {
        SeedVendor("V001", "Existing Vendor");
        var handler = new CreateVendorCommandHandler(_context);

        var act = () => handler.Handle(new CreateVendorCommand(ValidCreateDto("V001")), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*V001*");
    }

    [Fact]
    public async Task Create_allows_same_code_as_soft_deleted_vendor()
    {
        SeedVendor("V001", "Old Vendor", isDeleted: true);
        var handler = new CreateVendorCommandHandler(_context);

        var result = await handler.Handle(new CreateVendorCommand(ValidCreateDto("V001")), default);
        result.VendorCode.Should().Be("V001");
    }

    // --- UpdateVendorCommandHandler ---

    [Fact]
    public async Task Update_modifies_fields_and_returns_dto()
    {
        var vendor = SeedVendor("V002", "Old Name");
        var dto = new UpdateVendorDto { Name = "New Name", PaymentTermsDays = 60, IsActive = true };
        var handler = new UpdateVendorCommandHandler(_context);

        var result = await handler.Handle(new UpdateVendorCommand(vendor.Id, dto), default);

        result.Name.Should().Be("New Name");
        result.PaymentTermsDays.Should().Be(60);
    }

    [Fact]
    public async Task Update_throws_NotFoundException_when_vendor_not_found()
    {
        var handler = new UpdateVendorCommandHandler(_context);
        var act = () => handler.Handle(new UpdateVendorCommand(Guid.NewGuid(), new UpdateVendorDto { Name = "X" }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_throws_NotFoundException_for_soft_deleted_vendor()
    {
        var vendor = SeedVendor("V003", "Deleted", isDeleted: true);
        var handler = new UpdateVendorCommandHandler(_context);
        var act = () => handler.Handle(new UpdateVendorCommand(vendor.Id, new UpdateVendorDto { Name = "X" }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- DeleteVendorCommandHandler ---

    [Fact]
    public async Task Delete_soft_deletes_vendor()
    {
        var vendor = SeedVendor("V004", "To Delete");
        var handler = new DeleteVendorCommandHandler(_context);

        var result = await handler.Handle(new DeleteVendorCommand(vendor.Id), default);

        result.Should().Be(Unit.Value);
        _context.Set<Vendor>().First(v => v.Id == vendor.Id).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_throws_NotFoundException_when_not_found()
    {
        var handler = new DeleteVendorCommandHandler(_context);
        var act = () => handler.Handle(new DeleteVendorCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- GetVendorsQueryHandler ---

    [Fact]
    public async Task GetAll_returns_only_active_vendors()
    {
        SeedVendor("V001", "Alpha");
        SeedVendor("V002", "Beta");
        SeedVendor("V003", "Deleted", isDeleted: true);
        var handler = new GetVendorsQueryHandler(_context);

        var result = await handler.Handle(new GetVendorsQuery(new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().NotContain(v => v.VendorCode == "V003");
    }

    [Fact]
    public async Task GetAll_filters_by_search_name()
    {
        SeedVendor("V001", "Alpha Corp");
        SeedVendor("V002", "Beta Ltd");
        var handler = new GetVendorsQueryHandler(_context);

        var result = await handler.Handle(
            new GetVendorsQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "Alpha" }), default);

        result.TotalCount.Should().Be(1);
        result.Items[0].Name.Should().Be("Alpha Corp");
    }

    [Fact]
    public async Task GetAll_filters_by_vendor_code()
    {
        SeedVendor("VND-100", "Vendor One");
        SeedVendor("VND-200", "Vendor Two");
        var handler = new GetVendorsQueryHandler(_context);

        var result = await handler.Handle(
            new GetVendorsQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "VND-200" }), default);

        result.TotalCount.Should().Be(1);
        result.Items[0].VendorCode.Should().Be("VND-200");
    }

    [Fact]
    public async Task GetAll_sorts_ascending_by_name()
    {
        SeedVendor("V002", "Zeta");
        SeedVendor("V001", "Alpha");
        var handler = new GetVendorsQueryHandler(_context);

        var result = await handler.Handle(
            new GetVendorsQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = false }), default);

        result.Items[0].Name.Should().Be("Alpha");
        result.Items[1].Name.Should().Be("Zeta");
    }

    [Fact]
    public async Task GetAll_sorts_descending_by_name()
    {
        SeedVendor("V002", "Zeta");
        SeedVendor("V001", "Alpha");
        var handler = new GetVendorsQueryHandler(_context);

        var result = await handler.Handle(
            new GetVendorsQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = true }), default);

        result.Items[0].Name.Should().Be("Zeta");
    }

    [Fact]
    public async Task GetAll_paginates_correctly()
    {
        for (int i = 1; i <= 6; i++) SeedVendor($"V{i:D3}", $"Vendor {i:D2}");
        var handler = new GetVendorsQueryHandler(_context);

        var result = await handler.Handle(
            new GetVendorsQuery(new PaginationRequest { Page = 2, PageSize = 4 }), default);

        result.TotalCount.Should().Be(6);
        result.Items.Should().HaveCount(2);
    }

    // --- GetVendorByIdQueryHandler ---

    [Fact]
    public async Task GetById_returns_vendor()
    {
        var vendor = SeedVendor("V005", "Target Vendor");
        var handler = new GetVendorByIdQueryHandler(_context);

        var result = await handler.Handle(new GetVendorByIdQuery(vendor.Id), default);

        result.Id.Should().Be(vendor.Id);
        result.Name.Should().Be("Target Vendor");
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_missing_vendor()
    {
        var handler = new GetVendorByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetVendorByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_soft_deleted_vendor()
    {
        var vendor = SeedVendor("V006", "Deleted", isDeleted: true);
        var handler = new GetVendorByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetVendorByIdQuery(vendor.Id), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
