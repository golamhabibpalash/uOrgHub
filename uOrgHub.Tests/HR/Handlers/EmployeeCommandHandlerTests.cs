using FluentAssertions;
using MediatR;
using Moq;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Features.CoreHR.Commands;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.HR.Repositories;
using uOrgHub.Shared.Exceptions;

namespace uOrgHub.Tests.HR.Handlers;

public class CreateEmployeeCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repo = new();
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandHandlerTests()
    {
        _handler = new CreateEmployeeCommandHandler(_repo.Object);
    }

    private CreateEmployeeDto ValidDto() => new()
    {
        EmployeeCode = "EMP001",
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        DesignationId = Guid.NewGuid(),
        DepartmentId = Guid.NewGuid(),
        JoiningDate = DateTime.UtcNow,
        BasicSalary = 50000
    };

    private Employee EntityFrom(CreateEmployeeDto dto) => new()
    {
        Id = Guid.NewGuid(),
        EmployeeCode = dto.EmployeeCode,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        DesignationId = dto.DesignationId,
        DepartmentId = dto.DepartmentId,
        JoiningDate = dto.JoiningDate,
        BasicSalary = dto.BasicSalary,
        Designation = new Designation { Id = dto.DesignationId, Name = "Engineer", Code = "ENG", Level = 1, DepartmentId = dto.DepartmentId, Department = new Department { Id = dto.DepartmentId, Name = "Engineering", Code = "ENG", Type = DepartmentType.Technical } },
        Department = new Department { Id = dto.DepartmentId, Name = "Engineering", Code = "ENG", Type = DepartmentType.Technical }
    };

    [Fact]
    public async Task Creates_employee_when_code_and_email_are_unique()
    {
        var dto = ValidDto();
        var entity = EntityFrom(dto);

        _repo.Setup(r => r.CodeExistsAsync(dto.EmployeeCode, null)).ReturnsAsync(false);
        _repo.Setup(r => r.EmailExistsAsync(dto.Email, null)).ReturnsAsync(false);
        _repo.Setup(r => r.CreateAsync(It.IsAny<Employee>())).ReturnsAsync(entity);

        var result = await _handler.Handle(new CreateEmployeeCommand(dto), CancellationToken.None);

        result.Should().NotBeNull();
        result.EmployeeCode.Should().Be(dto.EmployeeCode);
        _repo.Verify(r => r.CreateAsync(It.IsAny<Employee>()), Times.Once);
    }

    [Fact]
    public async Task Throws_AppException_when_employee_code_already_exists()
    {
        var dto = ValidDto();
        _repo.Setup(r => r.CodeExistsAsync(dto.EmployeeCode, null)).ReturnsAsync(true);

        var act = () => _handler.Handle(new CreateEmployeeCommand(dto), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage($"*{dto.EmployeeCode}*");
        _repo.Verify(r => r.CreateAsync(It.IsAny<Employee>()), Times.Never);
    }

    [Fact]
    public async Task Throws_AppException_when_email_already_in_use()
    {
        var dto = ValidDto();
        _repo.Setup(r => r.CodeExistsAsync(dto.EmployeeCode, null)).ReturnsAsync(false);
        _repo.Setup(r => r.EmailExistsAsync(dto.Email, null)).ReturnsAsync(true);

        var act = () => _handler.Handle(new CreateEmployeeCommand(dto), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage($"*{dto.Email}*");
        _repo.Verify(r => r.CreateAsync(It.IsAny<Employee>()), Times.Never);
    }
}

public class UpdateEmployeeCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repo = new();
    private readonly UpdateEmployeeCommandHandler _handler;

    public UpdateEmployeeCommandHandlerTests()
    {
        _handler = new UpdateEmployeeCommandHandler(_repo.Object);
    }

    private Employee ExistingEmployee(Guid id)
    {
        var deptId = Guid.NewGuid();
        var desiId = Guid.NewGuid();
        var dept = new Department { Id = deptId, Name = "Engineering", Code = "ENG", Type = DepartmentType.Technical };
        return new Employee
        {
            Id = id, EmployeeCode = "EMP001", FirstName = "John", LastName = "Doe",
            Email = "john@test.com", DesignationId = desiId, DepartmentId = deptId,
            JoiningDate = DateTime.UtcNow.AddYears(-1), BasicSalary = 40000,
            Designation = new Designation { Id = desiId, Name = "Engineer", Code = "ENG", Level = 1, DepartmentId = deptId, Department = dept },
            Department = dept
        };
    }

    [Fact]
    public async Task Throws_NotFoundException_when_employee_not_found()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Employee?)null);

        var act = () => _handler.Handle(
            new UpdateEmployeeCommand(id, new UpdateEmployeeDto { Email = "x@x.com" }),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_AppException_when_email_is_taken_by_another_employee()
    {
        var id = Guid.NewGuid();
        var entity = ExistingEmployee(id);
        var dto = new UpdateEmployeeDto { Email = "taken@test.com" };

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(r => r.EmailExistsAsync(dto.Email, id)).ReturnsAsync(true);

        var act = () => _handler.Handle(new UpdateEmployeeCommand(id, dto), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage($"*{dto.Email}*");
    }

    [Fact]
    public async Task Updates_employee_when_valid()
    {
        var id = Guid.NewGuid();
        var entity = ExistingEmployee(id);
        var dto = new UpdateEmployeeDto { Email = "new@test.com" };

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(r => r.EmailExistsAsync(dto.Email, id)).ReturnsAsync(false);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Employee>())).ReturnsAsync(entity);

        var result = await _handler.Handle(new UpdateEmployeeCommand(id, dto), CancellationToken.None);

        result.Should().NotBeNull();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Employee>()), Times.Once);
    }
}

public class DeleteEmployeeCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repo = new();
    private readonly DeleteEmployeeCommandHandler _handler;

    public DeleteEmployeeCommandHandlerTests()
    {
        _handler = new DeleteEmployeeCommandHandler(_repo.Object);
    }

    [Fact]
    public async Task Deletes_employee_when_it_exists()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _repo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new DeleteEmployeeCommand(id), CancellationToken.None);

        result.Should().Be(Unit.Value);
        _repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_employee_does_not_exist()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

        var act = () => _handler.Handle(new DeleteEmployeeCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
