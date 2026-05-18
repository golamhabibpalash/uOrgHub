using FluentAssertions;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.DTOs.Validators;

namespace uOrgHub.Tests.Accounts.Validators;

public class CreateFiscalYearValidatorTests
{
    private readonly CreateFiscalYearValidator _validator = new();

    private CreateFiscalYearDto ValidDto() => new()
    {
        Name = "FY 2026",
        StartDate = new DateTime(2026, 1, 1),
        EndDate = new DateTime(2026, 12, 31)
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
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name is required");
    }

    [Fact]
    public void Name_over_50_chars_fails()
    {
        var dto = ValidDto(); dto.Name = new string('N', 51);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Default_start_date_fails()
    {
        var dto = ValidDto(); dto.StartDate = default;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Start date is required");
    }

    [Fact]
    public void Default_end_date_fails()
    {
        var dto = ValidDto(); dto.EndDate = default;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "End date is required");
    }

    [Fact]
    public void End_date_before_start_date_fails()
    {
        var dto = ValidDto();
        dto.StartDate = new DateTime(2026, 12, 31);
        dto.EndDate = new DateTime(2026, 1, 1);
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "End date must be after start date");
    }

    [Fact]
    public void End_date_equal_to_start_date_fails()
    {
        var dto = ValidDto();
        dto.StartDate = new DateTime(2026, 6, 1);
        dto.EndDate = new DateTime(2026, 6, 1);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
