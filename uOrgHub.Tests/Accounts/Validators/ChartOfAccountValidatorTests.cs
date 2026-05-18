using FluentAssertions;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.DTOs.Validators;
using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Tests.Accounts.Validators;

public class CreateChartOfAccountValidatorTests
{
    private readonly CreateChartOfAccountValidator _validator = new();

    private CreateChartOfAccountDto ValidDto() => new()
    {
        AccountCode = "1001",
        AccountName = "Cash and Cash Equivalents",
        AccountGroupId = Guid.NewGuid()
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_account_code_fails()
    {
        var dto = ValidDto(); dto.AccountCode = "";
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Account Code is required");
    }

    [Fact]
    public void Account_code_over_20_chars_fails()
    {
        var dto = ValidDto(); dto.AccountCode = new string('1', 21);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_account_name_fails()
    {
        var dto = ValidDto(); dto.AccountName = "";
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Account Name is required");
    }

    [Fact]
    public void Account_name_over_200_chars_fails()
    {
        var dto = ValidDto(); dto.AccountName = new string('A', 201);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_account_group_id_fails()
    {
        var dto = ValidDto(); dto.AccountGroupId = Guid.Empty;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Account Group is required");
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
