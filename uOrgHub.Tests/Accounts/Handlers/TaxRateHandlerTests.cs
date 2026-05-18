using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.TaxRate;
using uOrgHub.Accounts.Features.TaxRate;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Handlers;

public class TaxRateHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public TaxRateHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
    }

    public void Dispose() => _context.Dispose();

    private TaxRate SeedTaxRate(string code, string name, decimal rate = 15m, bool isDeleted = false)
    {
        var tr = new TaxRate
        {
            Id = Guid.NewGuid(), Code = code, Name = name,
            TaxType = TaxType.VAT, Rate = rate,
            IsActive = true, IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<TaxRate>().Add(tr);
        _context.SaveChanges();
        return tr;
    }

    private CreateTaxRateDto ValidCreateDto(string code = "VAT-15") => new()
    {
        Code = code,
        Name = "Standard VAT",
        TaxType = TaxType.VAT,
        Rate = 15m
    };

    // --- CreateTaxRateCommandHandler ---

    [Fact]
    public async Task Create_saves_tax_rate_and_returns_dto()
    {
        var handler = new CreateTaxRateCommandHandler(_context);

        var result = await handler.Handle(new CreateTaxRateCommand(ValidCreateDto("VAT-15")), default);

        result.Code.Should().Be("VAT-15");
        result.Name.Should().Be("Standard VAT");
        result.Rate.Should().Be(15m);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_throws_when_code_already_exists()
    {
        SeedTaxRate("VAT-15", "Existing");
        var handler = new CreateTaxRateCommandHandler(_context);

        var act = () => handler.Handle(new CreateTaxRateCommand(ValidCreateDto("VAT-15")), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*VAT-15*");
    }

    [Fact]
    public async Task Create_allows_same_code_as_soft_deleted_tax_rate()
    {
        SeedTaxRate("VAT-15", "Old", isDeleted: true);
        var handler = new CreateTaxRateCommandHandler(_context);

        var result = await handler.Handle(new CreateTaxRateCommand(ValidCreateDto("VAT-15")), default);
        result.Code.Should().Be("VAT-15");
    }

    // --- UpdateTaxRateCommandHandler ---

    [Fact]
    public async Task Update_modifies_fields_and_returns_dto()
    {
        var tr = SeedTaxRate("WHT-10", "Withholding", 10m);
        var dto = new UpdateTaxRateDto { Name = "Updated WHT", TaxType = TaxType.WithholdingTax, Rate = 12m, IsActive = true };
        var handler = new UpdateTaxRateCommandHandler(_context);

        var result = await handler.Handle(new UpdateTaxRateCommand(tr.Id, dto), default);

        result.Name.Should().Be("Updated WHT");
        result.Rate.Should().Be(12m);
        result.TaxType.Should().Be(TaxType.WithholdingTax);
    }

    [Fact]
    public async Task Update_throws_NotFoundException_when_not_found()
    {
        var handler = new UpdateTaxRateCommandHandler(_context);
        var act = () => handler.Handle(new UpdateTaxRateCommand(Guid.NewGuid(), new UpdateTaxRateDto { Name = "X", TaxType = TaxType.VAT, Rate = 5m, IsActive = true }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_throws_NotFoundException_for_soft_deleted_tax_rate()
    {
        var tr = SeedTaxRate("VAT-OLD", "Deleted", isDeleted: true);
        var handler = new UpdateTaxRateCommandHandler(_context);
        var act = () => handler.Handle(new UpdateTaxRateCommand(tr.Id, new UpdateTaxRateDto { Name = "X", TaxType = TaxType.VAT, Rate = 5m, IsActive = true }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- DeleteTaxRateCommandHandler ---

    [Fact]
    public async Task Delete_soft_deletes_tax_rate()
    {
        var tr = SeedTaxRate("VAT-DEL", "To Delete");
        var handler = new DeleteTaxRateCommandHandler(_context);

        var result = await handler.Handle(new DeleteTaxRateCommand(tr.Id), default);

        result.Should().Be(Unit.Value);
        _context.Set<TaxRate>().First(t => t.Id == tr.Id).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_throws_NotFoundException_when_not_found()
    {
        var handler = new DeleteTaxRateCommandHandler(_context);
        var act = () => handler.Handle(new DeleteTaxRateCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- GetTaxRatesQueryHandler ---

    [Fact]
    public async Task GetAll_excludes_deleted_tax_rates()
    {
        SeedTaxRate("VAT-15", "Standard VAT");
        SeedTaxRate("WHT-10", "Withholding");
        SeedTaxRate("VAT-DEL", "Deleted", isDeleted: true);
        var handler = new GetTaxRatesQueryHandler(_context);

        var result = await handler.Handle(new GetTaxRatesQuery(new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAll_filters_by_name_search()
    {
        SeedTaxRate("VAT-15", "Standard VAT");
        SeedTaxRate("WHT-10", "Withholding Tax");
        var handler = new GetTaxRatesQueryHandler(_context);

        var result = await handler.Handle(
            new GetTaxRatesQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "Withholding" }), default);

        result.TotalCount.Should().Be(1);
        result.Items[0].Name.Should().Be("Withholding Tax");
    }

    [Fact]
    public async Task GetAll_filters_by_code_search()
    {
        SeedTaxRate("VAT-15", "Standard VAT");
        SeedTaxRate("WHT-10", "Withholding Tax");
        var handler = new GetTaxRatesQueryHandler(_context);

        var result = await handler.Handle(
            new GetTaxRatesQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "WHT-10" }), default);

        result.TotalCount.Should().Be(1);
        result.Items[0].Code.Should().Be("WHT-10");
    }

    [Fact]
    public async Task GetAll_sorts_ascending_by_name()
    {
        SeedTaxRate("T-002", "Zeta Tax");
        SeedTaxRate("T-001", "Alpha Tax");
        var handler = new GetTaxRatesQueryHandler(_context);

        var result = await handler.Handle(
            new GetTaxRatesQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = false }), default);

        result.Items[0].Name.Should().Be("Alpha Tax");
    }

    [Fact]
    public async Task GetAll_sorts_descending_by_name()
    {
        SeedTaxRate("T-002", "Zeta Tax");
        SeedTaxRate("T-001", "Alpha Tax");
        var handler = new GetTaxRatesQueryHandler(_context);

        var result = await handler.Handle(
            new GetTaxRatesQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = true }), default);

        result.Items[0].Name.Should().Be("Zeta Tax");
    }

    [Fact]
    public async Task GetAll_paginates_correctly()
    {
        for (int i = 1; i <= 6; i++) SeedTaxRate($"T-{i:D3}", $"Tax Rate {i:D2}", i * 2m);
        var handler = new GetTaxRatesQueryHandler(_context);

        var result = await handler.Handle(
            new GetTaxRatesQuery(new PaginationRequest { Page = 2, PageSize = 4 }), default);

        result.TotalCount.Should().Be(6);
        result.Items.Should().HaveCount(2);
    }

    // --- GetTaxRateByIdQueryHandler ---

    [Fact]
    public async Task GetById_returns_correct_tax_rate()
    {
        var tr = SeedTaxRate("VAT-20", "High VAT", 20m);
        var handler = new GetTaxRateByIdQueryHandler(_context);

        var result = await handler.Handle(new GetTaxRateByIdQuery(tr.Id), default);

        result.Id.Should().Be(tr.Id);
        result.Code.Should().Be("VAT-20");
        result.Rate.Should().Be(20m);
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_missing_tax_rate()
    {
        var handler = new GetTaxRateByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetTaxRateByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_deleted_tax_rate()
    {
        var tr = SeedTaxRate("VAT-DEL2", "Deleted", isDeleted: true);
        var handler = new GetTaxRateByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetTaxRateByIdQuery(tr.Id), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
