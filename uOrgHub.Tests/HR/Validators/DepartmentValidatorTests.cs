using FluentAssertions;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.DTOs.Validators;
using uOrgHub.HR.Models.Enums;

namespace uOrgHub.Tests.HR.Validators;

public class CreateDepartmentDtoValidatorTests
{
    private readonly CreateDepartmentDtoValidator _validator = new();

    [Fact]
    public void Valid_dto_passes()
    {
        var dto = new CreateDepartmentDto { Name = "Engineering", Code = "ENG", Type = DepartmentType.Technical };
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var dto = new CreateDepartmentDto { Name = "", Code = "ENG" };
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Name));
    }

    [Fact]
    public void Name_exceeding_100_chars_fails()
    {
        var dto = new CreateDepartmentDto { Name = new string('A', 101), Code = "ENG" };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_code_fails()
    {
        var dto = new CreateDepartmentDto { Name = "Engineering", Code = "" };
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(dto.Code));
    }

    [Fact]
    public void Code_exceeding_20_chars_fails()
    {
        var dto = new CreateDepartmentDto { Name = "Engineering", Code = new string('X', 21) };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Description_within_500_chars_passes()
    {
        var dto = new CreateDepartmentDto { Name = "HR", Code = "HR", Description = new string('D', 500) };
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Description_exceeding_500_chars_fails()
    {
        var dto = new CreateDepartmentDto { Name = "HR", Code = "HR", Description = new string('D', 501) };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Null_description_passes()
    {
        var dto = new CreateDepartmentDto { Name = "Finance", Code = "FIN", Description = null };
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }
}

public class UpdateDepartmentDtoValidatorTests
{
    private readonly UpdateDepartmentDtoValidator _validator = new();

    [Fact]
    public void Valid_update_passes()
    {
        var dto = new UpdateDepartmentDto { Name = "Finance", Code = "FIN" };
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var dto = new UpdateDepartmentDto { Name = "", Code = "FIN" };
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
