using FluentAssertions;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.DTOs.Validators;

namespace uOrgHub.Tests.HR.Validators;

public class CreateDesignationDtoValidatorTests
{
    private readonly CreateDesignationDtoValidator _validator = new();

    private CreateDesignationDto ValidDto() => new()
    {
        Name = "Software Engineer",
        Code = "SE",
        DepartmentId = Guid.NewGuid(),
        Level = 1
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var dto = ValidDto(); dto.Name = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Name_over_100_chars_fails()
    {
        var dto = ValidDto(); dto.Name = new string('X', 101);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_code_fails()
    {
        var dto = ValidDto(); dto.Code = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Code_over_20_chars_fails()
    {
        var dto = ValidDto(); dto.Code = new string('C', 21);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_departmentId_fails()
    {
        var dto = ValidDto(); dto.DepartmentId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Level_zero_fails()
    {
        var dto = ValidDto(); dto.Level = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Level_one_passes()
    {
        var dto = ValidDto(); dto.Level = 1;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Negative_level_fails()
    {
        var dto = ValidDto(); dto.Level = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
