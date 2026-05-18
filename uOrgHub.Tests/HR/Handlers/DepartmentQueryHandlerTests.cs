using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.HR.Features.CoreHR.Queries;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;
using uOrgHub.Shared.Exceptions;
using uOrgHub.Shared.Models;

namespace uOrgHub.Tests.HR.Handlers;

public class DepartmentQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _context;

    public DepartmentQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    public void Dispose() => _context.Dispose();

    private Department SeedDepartment(string name, string code, bool isDeleted = false)
    {
        var dept = new Department
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Type = DepartmentType.Other,
            IsActive = true,
            IsDeleted = isDeleted
        };
        _context.Set<Department>().Add(dept);
        _context.SaveChanges();
        return dept;
    }

    // --- GetDepartmentsQueryHandler ---

    [Fact]
    public async Task Returns_paged_departments_excluding_deleted()
    {
        SeedDepartment("Engineering", "ENG");
        SeedDepartment("Finance", "FIN");
        SeedDepartment("Deleted Dept", "DEL", isDeleted: true);

        var handler = new GetDepartmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetDepartmentsQuery(new PaginationRequest { Page = 1, PageSize = 10 }),
            CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().NotContain(d => d.Code == "DEL");
    }

    [Fact]
    public async Task Filters_departments_by_search_term()
    {
        SeedDepartment("Engineering", "ENG");
        SeedDepartment("Finance", "FIN");

        var handler = new GetDepartmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetDepartmentsQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "Engi" }),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items[0].Name.Should().Be("Engineering");
    }

    [Fact]
    public async Task Searches_by_code()
    {
        SeedDepartment("Engineering", "ENG");
        SeedDepartment("Finance", "FIN");

        var handler = new GetDepartmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetDepartmentsQuery(new PaginationRequest { Page = 1, PageSize = 10, Search = "FIN" }),
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items[0].Code.Should().Be("FIN");
    }

    [Fact]
    public async Task Paginates_correctly()
    {
        for (int i = 1; i <= 5; i++)
            SeedDepartment($"Dept {i:D2}", $"D{i:D2}");

        var handler = new GetDepartmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetDepartmentsQuery(new PaginationRequest { Page = 1, PageSize = 3 }),
            CancellationToken.None);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task Returns_second_page_correctly()
    {
        for (int i = 1; i <= 5; i++)
            SeedDepartment($"Dept {i:D2}", $"D{i:D2}");

        var handler = new GetDepartmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetDepartmentsQuery(new PaginationRequest { Page = 2, PageSize = 3 }),
            CancellationToken.None);

        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Sorts_ascending_by_name_by_default()
    {
        SeedDepartment("Zeta", "ZET");
        SeedDepartment("Alpha", "ALP");

        var handler = new GetDepartmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetDepartmentsQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = false }),
            CancellationToken.None);

        result.Items[0].Name.Should().Be("Alpha");
        result.Items[1].Name.Should().Be("Zeta");
    }

    [Fact]
    public async Task Sorts_descending_when_requested()
    {
        SeedDepartment("Zeta", "ZET");
        SeedDepartment("Alpha", "ALP");

        var handler = new GetDepartmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetDepartmentsQuery(new PaginationRequest { Page = 1, PageSize = 10, SortDescending = true }),
            CancellationToken.None);

        result.Items[0].Name.Should().Be("Zeta");
        result.Items[1].Name.Should().Be("Alpha");
    }

    // --- GetDepartmentByIdQueryHandler ---

    [Fact]
    public async Task Returns_department_by_id()
    {
        var dept = SeedDepartment("HR", "HR");

        var handler = new GetDepartmentByIdQueryHandler(_context);
        var result = await handler.Handle(new GetDepartmentByIdQuery(dept.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(dept.Id);
        result.Name.Should().Be("HR");
    }

    [Fact]
    public async Task Throws_NotFoundException_when_department_not_found()
    {
        var handler = new GetDepartmentByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetDepartmentByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_NotFoundException_for_soft_deleted_department()
    {
        var dept = SeedDepartment("Deleted", "DEL", isDeleted: true);

        var handler = new GetDepartmentByIdQueryHandler(_context);
        var act = () => handler.Handle(new GetDepartmentByIdQuery(dept.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Returns_empty_list_when_no_departments_exist()
    {
        var handler = new GetDepartmentsQueryHandler(_context);
        var result = await handler.Handle(
            new GetDepartmentsQuery(new PaginationRequest { Page = 1, PageSize = 10 }),
            CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}
