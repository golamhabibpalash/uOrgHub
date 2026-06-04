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

public class CreateDesignationCommandHandlerTests
{
    private readonly Mock<IDesignationRepository> _repo = new();
    private readonly CreateDesignationCommandHandler _handler;

    public CreateDesignationCommandHandlerTests()
    {
        _handler = new CreateDesignationCommandHandler(_repo.Object);
    }

    private static Department DummyDepartment() => new()
    {
        Id = Guid.NewGuid(), Name = "Engineering", Code = "ENG", Type = DepartmentType.Technical
    };

    private CreateDesignationDto ValidDto() => new()
    {
        Name = "Senior Engineer",
        Code = "SE",
        DepartmentId = Guid.NewGuid(),
        Level = 3
    };

    [Fact]
    public async Task Creates_designation_when_code_is_unique()
    {
        var dto = ValidDto();
        var entity = new Designation { Id = Guid.NewGuid(), Name = dto.Name, Code = dto.Code, DepartmentId = dto.DepartmentId, Level = dto.Level, Department = DummyDepartment() };

        _repo.Setup(r => r.CodeExistsAsync(dto.Code, null)).ReturnsAsync(false);
        _repo.Setup(r => r.CreateAsync(It.IsAny<Designation>())).ReturnsAsync(entity);

        var result = await _handler.Handle(new CreateDesignationCommand(dto), CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be(dto.Code);
        _repo.Verify(r => r.CreateAsync(It.IsAny<Designation>()), Times.Once);
    }

    [Fact]
    public async Task Throws_AppException_when_code_already_exists()
    {
        var dto = ValidDto();
        _repo.Setup(r => r.CodeExistsAsync(dto.Code, null)).ReturnsAsync(true);

        var act = () => _handler.Handle(new CreateDesignationCommand(dto), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>()
            .WithMessage($"*{dto.Code}*");
    }
}

public class UpdateDesignationCommandHandlerTests
{
    private readonly Mock<IDesignationRepository> _repo = new();
    private readonly UpdateDesignationCommandHandler _handler;

    public UpdateDesignationCommandHandlerTests()
    {
        _handler = new UpdateDesignationCommandHandler(_repo.Object);
    }

    private static Designation EntityWithDept(Guid id) => new()
    {
        Id = id, Name = "Old", Code = "OLD", DepartmentId = Guid.NewGuid(), Level = 1,
        Department = new Department { Id = Guid.NewGuid(), Name = "Engineering", Code = "ENG", Type = DepartmentType.Technical }
    };

    [Fact]
    public async Task Throws_NotFoundException_when_designation_not_found()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Designation?)null);

        var act = () => _handler.Handle(
            new UpdateDesignationCommand(id, new UpdateDesignationDto { Name = "X", Code = "X" }),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Throws_AppException_when_code_conflicts()
    {
        var id = Guid.NewGuid();
        var entity = EntityWithDept(id);
        var dto = new UpdateDesignationDto { Name = "New", Code = "TAKEN" };

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(r => r.CodeExistsAsync(dto.Code, id)).ReturnsAsync(true);

        var act = () => _handler.Handle(new UpdateDesignationCommand(id, dto), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Updates_when_entity_exists_and_code_is_unique()
    {
        var id = Guid.NewGuid();
        var entity = EntityWithDept(id);
        var dto = new UpdateDesignationDto { Name = "Updated", Code = "UPD" };

        _repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);
        _repo.Setup(r => r.CodeExistsAsync(dto.Code, id)).ReturnsAsync(false);
        _repo.Setup(r => r.UpdateAsync(It.IsAny<Designation>())).ReturnsAsync(entity);

        var result = await _handler.Handle(new UpdateDesignationCommand(id, dto), CancellationToken.None);

        result.Should().NotBeNull();
        _repo.Verify(r => r.UpdateAsync(It.IsAny<Designation>()), Times.Once);
    }
}

public class DeleteDesignationCommandHandlerTests
{
    private readonly Mock<IDesignationRepository> _repo = new();
    private readonly DeleteDesignationCommandHandler _handler;

    public DeleteDesignationCommandHandlerTests()
    {
        _handler = new DeleteDesignationCommandHandler(_repo.Object);
    }

    [Fact]
    public async Task Deletes_designation_when_it_exists_and_no_dependencies()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(true);
        _repo.Setup(r => r.GetDependenciesAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DesignationDependenciesDto { DesignationId = id, CanDelete = true });
        _repo.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new DeleteDesignationCommand(id), CancellationToken.None);

        result.Should().Be(Unit.Value);
        _repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task Throws_NotFoundException_when_designation_does_not_exist()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

        var act = () => _handler.Handle(new DeleteDesignationCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _repo.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
