using FluentAssertions;
using uOrgHub.Accounts.DTOs;
using uOrgHub.Accounts.DTOs.Validators;

namespace uOrgHub.Tests.Accounts.Validators;

public class CreateJournalEntryValidatorTests
{
    private readonly CreateJournalEntryValidator _validator = new();

    private CreateJournalEntryLineDto ValidLine(bool debit = true) => new()
    {
        AccountId = Guid.NewGuid(),
        DebitAmount = debit ? 1000 : 0,
        CreditAmount = debit ? 0 : 1000
    };

    private CreateJournalEntryDto ValidDto() => new()
    {
        Description = "Monthly office rent",
        EntryDate = DateTime.Today,
        Lines = new List<CreateJournalEntryLineDto> { ValidLine(debit: true), ValidLine(debit: false) }
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_description_fails()
    {
        var dto = ValidDto(); dto.Description = "";
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Description is required");
    }

    [Fact]
    public void Description_over_500_chars_fails()
    {
        var dto = ValidDto(); dto.Description = new string('D', 501);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_lines_fails()
    {
        var dto = ValidDto(); dto.Lines = new();
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Journal entry must have at least one line");
    }

    [Fact]
    public void Reference_number_within_50_chars_passes()
    {
        var dto = ValidDto(); dto.ReferenceNumber = new string('R', 50);
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Reference_number_over_50_chars_fails()
    {
        var dto = ValidDto(); dto.ReferenceNumber = new string('R', 51);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class CreateJournalEntryLineValidatorTests
{
    private readonly CreateJournalEntryLineValidator _validator = new();

    [Fact]
    public void Valid_debit_line_passes()
    {
        var line = new CreateJournalEntryLineDto { AccountId = Guid.NewGuid(), DebitAmount = 500 };
        _validator.Validate(line).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_credit_line_passes()
    {
        var line = new CreateJournalEntryLineDto { AccountId = Guid.NewGuid(), CreditAmount = 500 };
        _validator.Validate(line).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_account_id_fails()
    {
        var line = new CreateJournalEntryLineDto { AccountId = Guid.Empty, DebitAmount = 500 };
        var result = _validator.Validate(line);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Account is required");
    }

    [Fact]
    public void Both_debit_and_credit_zero_fails()
    {
        var line = new CreateJournalEntryLineDto { AccountId = Guid.NewGuid(), DebitAmount = 0, CreditAmount = 0 };
        var result = _validator.Validate(line);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Line must have either debit or credit amount");
    }

    [Fact]
    public void Both_debit_and_credit_non_zero_fails()
    {
        var line = new CreateJournalEntryLineDto { AccountId = Guid.NewGuid(), DebitAmount = 500, CreditAmount = 300 };
        var result = _validator.Validate(line);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Line cannot have both debit and credit amounts");
    }

    [Fact]
    public void Description_within_500_chars_passes()
    {
        var line = new CreateJournalEntryLineDto { AccountId = Guid.NewGuid(), DebitAmount = 100, Description = new string('D', 500) };
        _validator.Validate(line).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Description_over_500_chars_fails()
    {
        var line = new CreateJournalEntryLineDto { AccountId = Guid.NewGuid(), DebitAmount = 100, Description = new string('D', 501) };
        _validator.Validate(line).IsValid.Should().BeFalse();
    }
}
