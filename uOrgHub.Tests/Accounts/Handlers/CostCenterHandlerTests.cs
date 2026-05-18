using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Accounts.DTOs.CostCenter;
using uOrgHub.Accounts.Features.CostCenter;
using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.Accounts.Handlers;

public class CostCenterHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public CostCenterHandlerTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
    }

    public void Dispose() => _context.Dispose();

    private CostCenter SeedCostCenter(string code, string name, bool isDeleted = false, Guid? parentId = null)
    {
        var cc = new CostCenter
        {
            Id = Guid.NewGuid(), Code = code, Name = name,
            IsActive = true, IsDeleted = isDeleted,
            ParentCostCenterId = parentId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<CostCenter>().Add(cc);
        _context.SaveChanges();
        return cc;
    }

    private CreateCostCenterDto ValidCreateDto(string code = "CC-001") => new()
    {
        Code = code,
        Name = "Operations",
        Description = "Main operations center"
    };

    // --- CreateCostCenterCommandHandler ---

    [Fact]
    public async Task Create_saves_cost_center_and_returns_dto()
    {
        var handler = new CreateCostCenterCommandHandler(_context);

        var result = await handler.Handle(new CreateCostCenterCommand(ValidCreateDto("CC-001")), default);

        result.Code.Should().Be("CC-001");
        result.Name.Should().Be("Operations");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_throws_when_code_already_exists()
    {
        SeedCostCenter("CC-001", "Existing");
        var handler = new CreateCostCenterCommandHandler(_context);

        var act = () => handler.Handle(new CreateCostCenterCommand(ValidCreateDto("CC-001")), default);

        await act.Should().ThrowAsync<AppException>().WithMessage("*CC-001*");
    }

    [Fact]
    public async Task Create_allows_same_code_as_soft_deleted_cost_center()
    {
        SeedCostCenter("CC-001", "Old", isDeleted: true);
        var handler = new CreateCostCenterCommandHandler(_context);

        var result = await handler.Handle(new CreateCostCenterCommand(ValidCreateDto("CC-001")), default);
        result.Code.Should().Be("CC-001");
    }

    [Fact]
    public async Task Create_with_parent_sets_parent_id()
    {
        var parent = SeedCostCenter("CC-PARENT", "Parent");
        var dto = new CreateCostCenterDto { Code = "CC-CHILD", Name = "Child", ParentCostCenterId = parent.Id };
        var handler = new CreateCostCenterCommandHandler(_context);

        var result = await handler.Handle(new CreateCostCenterCommand(dto), default);

        result.ParentCostCenterId.Should().Be(parent.Id);
    }

    // --- UpdateCostCenterCommandHandler ---

    [Fact]
    public async Task Update_modifies_fields_and_returns_dto()
    {
        var cc = SeedCostCenter("CC-002", "Old Name");
        var dto = new UpdateCostCenterDto { Name = "New Name", IsActive = false };
        var handler = new UpdateCostCenterCommandHandler(_context);

        var result = await handler.Handle(new UpdateCostCenterCommand(cc.Id, dto), default);

        result.Name.Should().Be("New Name");
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Update_throws_NotFoundException_when_not_found()
    {
        var handler = new UpdateCostCenterCommandHandler(_context);
        var act = () => handler.Handle(new UpdateCostCenterCommand(Guid.NewGuid(), new UpdateCostCenterDto { Name = "X", IsActive = true }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_throws_NotFoundException_for_soft_deleted_cost_center()
    {
        var cc = SeedCostCenter("CC-003", "Deleted", isDeleted: true);
        var handler = new UpdateCostCenterCommandHandler(_context);
        var act = () => handler.Handle(new UpdateCostCenterCommand(cc.Id, new UpdateCostCenterDto { Name = "X", IsActive = true }), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- DeleteCostCenterCommandHandler ---

    [Fact]
    public async Task Delete_soft_deletes_cost_center()
    {
        var cc = SeedCostCenter("CC-004", "To Delete");
        var handler = new DeleteCostCenterCommandHandler(_context);

        var result = await handler.Handle(new DeleteCostCenterCommand(cc.Id), default);

        result.Should().Be(Unit.Value);
        _context.Set<CostCenter>().First(c => c.Id == cc.Id).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_throws_NotFoundException_when_not_found()
    {
        var handler = new DeleteCostCenterCommandHandler(_context);
        var act = () => handler.Handle(new DeleteCostCenterCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- GetCostCentersQueryHandler ---

    [Fact]
    public async Task GetAll_excludes_deleted_cost_centers()
    {
        SeedCostCenter("CC-001", "Alpha");
        SeedCostCenter("CC-002", "Beta");
        SeedCostCenter("CC-DEL", "Deleted", isDeleted: true);
        var handler = new GetCostCentersQueryHandler(_context);

        var result = await handler.Handle(new GetCostCentersQuery(new PaginationRequest { Page = 1, PageSize = 10 }), default);

        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAll_filters_by_name_search()
    {
        SeedCostCenter("CC-001", "Operations");
        SeedCostCenter("CC-002", "Marketing");
        var handler = new GetCostCentersQueryHandler(_context);

        var result = await handler.Handle(
            new GetCostCentersQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "Operations" }), default);

        result.TotalCount.Should().Be(1);
        result.Items[0].Name.Should().Be("Operations");
    }

    [Fact]
    public async Task GetAll_filters_by_code_search()
    {
        SeedCostCenter("CC-OPS", "Operations");
        SeedCostCenter("CC-MKT", "Marketing");
        var handler = new GetCostCentersQueryHandler(_context);

        var result = await handler.Handle(
            new GetCostCentersQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "CC-MKT" }), default);

        result.TotalCount.Should().Be(1);
        result.Items[0].Code.Should().Be("CC-MKT");
    }

    [Fact]
    public async Task GetAll_sorts_ascending_by_name()
    {
        SeedCostCenter("CC-002", "Zeta Dept");
        SeedCostCenter("CC-001", "Alpha Dept");
        var handler = new GetCostCentersQueryHandler(_context);

        var result = await handler.Handle(
            new GetCostCentersQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = false }), default);

        result.Items[0].Name.Should().Be("Alpha Dept");
    }

    [Fact]
    public async Task GetAll_sorts_descending_by_name()
    {
        SeedCostCenter("CC-002", "Zeta Dept");
        SeedCostCenter("CC-001", "Alpha Dept");
        var handler = new GetCostCentersQueryHandler(_context);

        var result = await handler.Handle(
            new GetCostCentersQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = true }), default);

        result.Items[0].Name.Should().Be("Zeta Dept");
    }

    [Fact]
    public async Task GetAll_paginates_correctly()
    {
        for (int i = 1; i <= 5; i++) SeedCostCenter($"CC-{i:D3}", $"Center {i:D2}");
        var handler = new GetCostCentersQueryHandler(_context);

        var page1 = await handler.Handle(new GetCostCentersQuery(new PaginationRequest { Page = 1, PageSize = 3 }), default);
        var page2 = await handler.Handle(new GetCostCentersQuery(new PaginationRequest { Page = 2, PageSize = 3 }), default);

        page1.Items.Should().HaveCount(3);
        page2.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
    }

    // --- GetCostCenterByIdQueryHandler ---

    [Fact]
    public async Task GetById_returns_correct_cost_center()
    {
        var cc = SeedCostCenter("CC-010", "Target Center");
        var handler = new GetCostCenterByIdQueryHandler(_context);

        var result = await handler.Handle(new GetCostCenterByIdQuery(cc.Id), default);

        result.Id.Should().Be(cc.Id);
        result.Code.Should().Be("CC-010");
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_missing_cost_center()
    {
        var handler = new GetCostCenterByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetCostCenterByIdQuery(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetById_throws_NotFoundException_for_deleted_cost_center()
    {
        var cc = SeedCostCenter("CC-011", "Deleted", isDeleted: true);
        var handler = new GetCostCenterByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetCostCenterByIdQuery(cc.Id), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
