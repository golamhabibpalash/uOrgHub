using FluentAssertions;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.DTOs.Validators;
using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Tests.Accounts.Validators;

public class CreateAccountGroupValidatorTests
{
    private readonly CreateAccountGroupValidator _validator = new();

    private CreateAccountGroupDto ValidDto() => new()
    {
        Name = "Current Assets",
        Type = AccountGroupType.Asset,
        IsActive = true
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
    public void Name_over_100_chars_fails()
    {
        var dto = ValidDto(); dto.Name = new string('A', 101);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Description_within_500_chars_passes()
    {
        var dto = ValidDto(); dto.Description = new string('D', 500);
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Description_over_500_chars_fails()
    {
        var dto = ValidDto(); dto.Description = new string('D', 501);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class UpdateAccountGroupValidatorTests
{
    private readonly UpdateAccountGroupValidator _validator = new();

    private UpdateAccountGroupDto ValidDto() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Fixed Assets",
        Code = "FA",
        Type = AccountGroupType.Asset
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_id_fails()
    {
        var dto = ValidDto(); dto.Id = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var dto = ValidDto(); dto.Name = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_code_fails()
    {
        var dto = ValidDto(); dto.Code = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
