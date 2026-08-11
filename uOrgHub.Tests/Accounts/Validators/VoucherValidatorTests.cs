using FluentAssertions;
using uOrgHub.Accounts.DTOs.Validators;
using uOrgHub.Accounts.DTOs.Voucher;
using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Tests.Accounts.Validators;

public class CreateVoucherValidatorTests
{
    private readonly CreateVoucherValidator _validator = new();

    private static CreateVoucherDto ValidDto() => new()
    {
        VoucherType = VoucherType.Credit,
        VoucherDate = new DateTime(2026, 8, 11),
        ProjectId = Guid.NewGuid(),
        Description = "Investor deposit",
        DebitAccountId = Guid.NewGuid(),
        CreditAccountId = Guid.NewGuid(),
        Amount = 50000m
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Zero_amount_fails()
    {
        var dto = ValidDto(); dto.Amount = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_amount_fails()
    {
        var dto = ValidDto(); dto.Amount = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Same_account_on_both_sides_fails()
    {
        var dto = ValidDto();
        dto.CreditAccountId = dto.DebitAccountId;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_description_fails()
    {
        var dto = ValidDto(); dto.Description = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Overhead_voucher_with_only_a_cost_center_passes()
    {
        var dto = ValidDto();
        dto.ProjectId = null;
        dto.CostCenterId = Guid.NewGuid();
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Neither_project_nor_cost_center_fails()
    {
        var dto = ValidDto();
        dto.ProjectId = null;
        dto.CostCenterId = null;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Both_project_and_cost_center_fails()
    {
        var dto = ValidDto();
        dto.CostCenterId = Guid.NewGuid();
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_guid_does_not_count_as_a_charge_target()
    {
        var dto = ValidDto();
        dto.ProjectId = Guid.Empty;
        dto.CostCenterId = null;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class UpdateVoucherValidatorTests
{
    private readonly UpdateVoucherValidator _validator = new();

    private static UpdateVoucherDto ValidDto() => new()
    {
        VoucherDate = new DateTime(2026, 8, 11),
        ProjectId = Guid.NewGuid(),
        Description = "Payment to supplier",
        DebitAccountId = Guid.NewGuid(),
        CreditAccountId = Guid.NewGuid(),
        Amount = 20000m
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Neither_project_nor_cost_center_fails()
    {
        var dto = ValidDto();
        dto.ProjectId = null;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Same_account_on_both_sides_fails()
    {
        var dto = ValidDto();
        dto.CreditAccountId = dto.DebitAccountId;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
