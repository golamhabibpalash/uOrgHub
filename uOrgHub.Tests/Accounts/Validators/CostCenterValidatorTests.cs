using FluentAssertions;
using uOrgHub.Accounts.DTOs.CostCenter;
using uOrgHub.Accounts.DTOs.Validators;

namespace uOrgHub.Tests.Accounts.Validators;

public class CreateCostCenterValidatorTests
{
    private readonly CreateCostCenterValidator _validator = new();

    private CreateCostCenterDto ValidDto() => new()
    {
        Code = "CC-001",
        Name = "Site Office",
        ProjectId = Guid.NewGuid()
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_code_passes_because_the_handler_generates_one()
    {
        var dto = ValidDto(); dto.Code = "";
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Code_over_20_chars_fails()
    {
        var dto = ValidDto(); dto.Code = new string('C', 21);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var dto = ValidDto(); dto.Name = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
