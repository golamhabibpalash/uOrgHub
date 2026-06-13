using FluentAssertions;
using uOrgHub.Accounts.DTOs.AP;
using uOrgHub.Accounts.DTOs.Validators;

namespace uOrgHub.Tests.Accounts.Validators;

public class CreateVendorValidatorTests
{
    private readonly CreateVendorValidator _validator = new();

    private CreateVendorDto ValidDto() => new()
    {
        VendorCode = "V001",
        Name = "Acme Supplies Ltd",
        PaymentTermsDays = 30,
        PayableAccountId = Guid.NewGuid()
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_vendor_code_allowed()
    {
        var dto = ValidDto(); dto.VendorCode = "";
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Vendor_code_over_20_chars_fails()
    {
        var dto = ValidDto(); dto.VendorCode = new string('V', 21);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var dto = ValidDto(); dto.Name = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Invalid_email_format_fails()
    {
        var dto = ValidDto(); dto.Email = "not-valid";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_email_passes()
    {
        var dto = ValidDto(); dto.Email = "vendor@acme.com";
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_email_skips_format_validation()
    {
        var dto = ValidDto(); dto.Email = "";
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Negative_payment_terms_fails()
    {
        var dto = ValidDto(); dto.PaymentTermsDays = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_payment_terms_passes()
    {
        var dto = ValidDto(); dto.PaymentTermsDays = 0;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_payable_account_id_fails()
    {
        var dto = ValidDto(); dto.PayableAccountId = Guid.Empty;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Payable account is required");
    }
}

public class CreateBillValidatorTests
{
    private readonly CreateBillValidator _validator = new();

    private CreateBillLineDto ValidLine() => new()
    {
        Description = "Office supplies",
        Quantity = 5,
        UnitPrice = 100,
        DiscountPercent = 0,
        ExpenseAccountId = Guid.NewGuid()
    };

    private CreateBillDto ValidDto() => new()
    {
        BillNumber = "BILL-001",
        VendorId = Guid.NewGuid(),
        FiscalYearId = Guid.NewGuid(),
        BillDate = DateTime.Today,
        DueDate = DateTime.Today.AddDays(30),
        Lines = new List<CreateBillLineDto> { ValidLine() }
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_bill_number_fails()
    {
        var dto = ValidDto(); dto.BillNumber = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_vendor_id_fails()
    {
        var dto = ValidDto(); dto.VendorId = Guid.Empty;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Vendor is required");
    }

    [Fact]
    public void Empty_fiscal_year_id_fails()
    {
        var dto = ValidDto(); dto.FiscalYearId = Guid.Empty;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Fiscal year is required");
    }

    [Fact]
    public void Due_date_before_bill_date_fails()
    {
        var dto = ValidDto();
        dto.BillDate = DateTime.Today;
        dto.DueDate = DateTime.Today.AddDays(-1);
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Due date must be on or after bill date");
    }

    [Fact]
    public void Same_bill_and_due_date_passes()
    {
        var dto = ValidDto();
        dto.BillDate = DateTime.Today;
        dto.DueDate = DateTime.Today;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_lines_fails()
    {
        var dto = ValidDto(); dto.Lines = new();
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Bill must have at least one line");
    }
}

public class CreateBillLineValidatorTests
{
    private readonly CreateBillLineValidator _validator = new();

    private CreateBillLineDto ValidLine() => new()
    {
        Description = "Service charge",
        Quantity = 2,
        UnitPrice = 500,
        DiscountPercent = 5,
        ExpenseAccountId = Guid.NewGuid()
    };

    [Fact]
    public void Valid_line_passes()
    {
        _validator.Validate(ValidLine()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_description_fails()
    {
        var line = ValidLine(); line.Description = "";
        _validator.Validate(line).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_quantity_fails()
    {
        var line = ValidLine(); line.Quantity = 0;
        _validator.Validate(line).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_unit_price_fails()
    {
        var line = ValidLine(); line.UnitPrice = -1;
        _validator.Validate(line).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Discount_over_100_fails()
    {
        var line = ValidLine(); line.DiscountPercent = 101;
        _validator.Validate(line).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Discount_exactly_100_passes()
    {
        var line = ValidLine(); line.DiscountPercent = 100;
        _validator.Validate(line).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_expense_account_id_fails()
    {
        var line = ValidLine(); line.ExpenseAccountId = Guid.Empty;
        var result = _validator.Validate(line);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Expense account is required");
    }
}
