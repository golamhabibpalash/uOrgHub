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

public class CreateDepartmentCommandHandlerTests
{
    private readonly Mock<IDepartmentRepository> _repo = new();
    private readonly CreateDepartmentCommandHandler _handler;

    public CreateDepartmentCommandHandlerTests()
    {
        _handler = new CreateDepartmentCommandHandler(_repo.Object);
    }

    private CreateDepartmentDto ValidDto() => new()
    {
        Name = "Engineering",
        Code = "ENG",
        Type = DepartmentType.Technical,
        IsActive = true
    };

    [Fact]
    public async Task Creates_department_when_code_is_unique()
    {
        var dto = ValidDto();
        var entity = new Department { Id = Guid.NewGuid(), Name = dto.Name, Code = dto.Code, Type = dto.Type, IsActive = dto.IsActive };

        _repo.Setup(r => r.CodeExistsAsync(dto.Code, null)).ReturnsAsync(false);
        _repo.Setup(r => r.CreateAsync(It.IsAny<Department>())).ReturnsAsync(entity);

        var result = await _handler.Handle(new CreateDepartmentCommand(dto), CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be(dto.Name);
        result.Code.Should().Be(dto.Code);
        _repo.Verify(r => r.CreateAsync(It.IsAny<Department>()), Times.Once);
    }

    [Fact]
    public async Task Throws_AppException_when_code_already_exists()
    {
        var dto = ValidDto();
        _repo.Setup(r => r.CodeExistsAsync(dto.Code, null)).ReturnsAsync(true);

        var act = () => _handler.Handle(new CreateDepartmentCommand(dto), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage($"*{dto.Code}*");
        _repo.Verify(r => r.CreateAsync(It.IsAny<Department>()), Times.Never);
    }
}

public class UpdateDepartmentCommandHandlerTests
{
    private readonly Mock<IDepartmentRepository> _repo = new();
    private readonly UpdateDepartmentCommandHandler _handler;

    public UpdateDepartmentCommandHandlerTests()
    {
        _handler = new UpdateDepartmentCommandHandler(_repo.Object);
    }

    private Department ExistingEntity(Guid id) => new()
    {
        Id = id, Name = "Old Name", Code = "OLD", Type = DepartmentType.Other, IsActive = true
    };

    [Fact]
    public async Task Updates_department_when_entity_exists_and_code_is_unique()
    {
        var id = Guid.NewGuid();
        var entity = ExistingEntity(id);
        var dto = new UpdateDepartmentDto { Name = "New Name", Code = "NEW" };

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(r => r.CodeExistsAsync(dto.Code, id)).ReturnsAsync(false);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Department>())).ReturnsAsync(entity);

        var result = await _handler.Handle(new UpdateDepartmentCommand(id, dto), CancellationToken.None);

        result.Should().NotBeNull();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Department>()), Times.Once);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_entity_does_not_exist()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Department?)null);

        var act = () => _handler.Handle(
            new UpdateDepartmentCommand(id, new UpdateDepartmentDto { Name = "X", Code = "X" }),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_AppException_when_code_already_used_by_another_department()
    {
        var id = Guid.NewGuid();
        var entity = ExistingEntity(id);
        var dto = new UpdateDepartmentDto { Name = "New", Code = "TAKEN" };

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(r => r.CodeExistsAsync(dto.Code, id)).ReturnsAsync(true);

        var act = () => _handler.Handle(new UpdateDepartmentCommand(id, dto), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }
}

public class DeleteDepartmentCommandHandlerTests
{
    private readonly Mock<IDepartmentRepository> _repo = new();
    private readonly DeleteDepartmentCommandHandler _handler;

    public DeleteDepartmentCommandHandlerTests()
    {
        _handler = new DeleteDepartmentCommandHandler(_repo.Object);
    }

    [Fact]
    public async Task Deletes_department_when_it_exists()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _repo.Setup(r => r.GetDependenciesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DepartmentDependenciesDto { CanDelete = true });
        _repo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new DeleteDepartmentCommand(id), CancellationToken.None);

        result.Should().Be(Unit.Value);
        _repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_department_does_not_exist()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

        var act = () => _handler.Handle(new DeleteDepartmentCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
