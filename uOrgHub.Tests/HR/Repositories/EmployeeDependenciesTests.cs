using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using uOrgHub.Auth.Models.Entities;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Data;

namespace uOrgHub.Tests.HR.Repositories;

public class EmployeeDependenciesTests
{
    private static AppDbContext NewContext()
        => TestDb.NewContext("TestDb_EmployeeDeps_" + Guid.NewGuid());

    private static Employee SeedEmployee(AppDbContext ctx)
    {
        var emp = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            DesignationId = Guid.NewGuid(),
            DepartmentId = Guid.NewGuid(),
            JoiningDate = DateTime.UtcNow.AddYears(-1),
            BasicSalary = 40000,
        };
        ctx.Set<Employee>().Add(emp);
        ctx.SaveChanges();
        return emp;
    }

    private static void SeedUser(AppDbContext ctx, Guid employeeId, bool isActive, bool isDeleted)
    {
        ctx.Set<ApplicationUser>().Add(new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Username = "jdoe",
            Email = "john@test.com",
            PasswordHash = "x",
            FirstName = "John",
            LastName = "Doe",
            EmployeeId = employeeId,
            IsActive = isActive,
            IsDeleted = isDeleted,
        });
        ctx.SaveChanges();
    }

    [Fact]
    public async Task Employee_with_no_links_can_be_deleted()
    {
        using var ctx = NewContext();
        var emp = SeedEmployee(ctx);

        var deps = await new EmployeeRepository(ctx).GetDependenciesAsync(emp.Id);

        deps.CanDelete.Should().BeTrue();
        deps.BlockingReason.Should().BeNull();
    }

    [Fact]
    public async Task Linked_user_account_blocks_delete_and_names_the_remedy()
    {
        using var ctx = NewContext();
        var emp = SeedEmployee(ctx);
        SeedUser(ctx, emp.Id, isActive: true, isDeleted: false);

        var deps = await new EmployeeRepository(ctx).GetDependenciesAsync(emp.Id);

        deps.HasUserAccount.Should().BeTrue();
        deps.CanDelete.Should().BeFalse();
        deps.BlockingReason.Should().Be(
            "Cannot delete employee — linked records exist: a linked user account "
            + "(remove it under Admin → Users, or set the employee to Inactive instead of deleting).");
    }

    [Fact]
    public async Task Deactivated_user_account_still_blocks_delete()
    {
        using var ctx = NewContext();
        var emp = SeedEmployee(ctx);
        SeedUser(ctx, emp.Id, isActive: false, isDeleted: false);

        var deps = await new EmployeeRepository(ctx).GetDependenciesAsync(emp.Id);

        deps.HasUserAccount.Should().BeTrue();
        deps.CanDelete.Should().BeFalse();
    }

    [Fact]
    public async Task Soft_deleted_user_account_does_not_block_delete()
    {
        using var ctx = NewContext();
        var emp = SeedEmployee(ctx);
        SeedUser(ctx, emp.Id, isActive: false, isDeleted: true);

        var deps = await new EmployeeRepository(ctx).GetDependenciesAsync(emp.Id);

        deps.HasUserAccount.Should().BeFalse();
        deps.CanDelete.Should().BeTrue();
    }
}
