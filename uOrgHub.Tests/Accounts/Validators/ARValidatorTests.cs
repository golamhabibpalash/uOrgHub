using FluentAssertions;
using uOrgHub.Accounts.DTOs.AR;
using uOrgHub.Accounts.DTOs.Validators;

namespace uOrgHub.Tests.Accounts.Validators;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    private CreateCustomerDto ValidDto() => new()
    {
        CustomerCode = "C001",
        Name = "TechCorp Ltd",
        CreditLimit = 500000,
        PaymentTermsDays = 30,
        ReceivableAccountId = Guid.NewGuid()
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_customer_code_fails()
    {
        var dto = ValidDto(); dto.CustomerCode = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Customer_code_over_20_chars_fails()
    {
        var dto = ValidDto(); dto.CustomerCode = new string('C', 21);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var dto = ValidDto(); dto.Name = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Name_over_200_chars_fails()
    {
        var dto = ValidDto(); dto.Name = new string('N', 201);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Invalid_email_fails()
    {
        var dto = ValidDto(); dto.Email = "bad-email";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Valid_email_passes()
    {
        var dto = ValidDto(); dto.Email = "cust@techcorp.com";
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_email_skips_format_validation()
    {
        var dto = ValidDto(); dto.Email = "";
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Negative_credit_limit_fails()
    {
        var dto = ValidDto(); dto.CreditLimit = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_credit_limit_passes()
    {
        var dto = ValidDto(); dto.CreditLimit = 0;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_receivable_account_id_fails()
    {
        var dto = ValidDto(); dto.ReceivableAccountId = Guid.Empty;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Receivable account is required");
    }
}

public class CreateInvoiceValidatorTests
{
    private readonly CreateInvoiceValidator _validator = new();

    private CreateInvoiceLineDto ValidLine() => new()
    {
        Description = "Consulting services",
        Quantity = 10,
        UnitPrice = 200,
        DiscountPercent = 0,
        RevenueAccountId = Guid.NewGuid()
    };

    private CreateInvoiceDto ValidDto() => new()
    {
        InvoiceNumber = "INV-001",
        CustomerId = Guid.NewGuid(),
        FiscalYearId = Guid.NewGuid(),
        InvoiceDate = DateTime.Today,
        DueDate = DateTime.Today.AddDays(30),
        Lines = new List<CreateInvoiceLineDto> { ValidLine() }
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_invoice_number_fails()
    {
        var dto = ValidDto(); dto.InvoiceNumber = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_customer_id_fails()
    {
        var dto = ValidDto(); dto.CustomerId = Guid.Empty;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Customer is required");
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
    public void Due_date_before_invoice_date_fails()
    {
        var dto = ValidDto();
        dto.InvoiceDate = DateTime.Today;
        dto.DueDate = DateTime.Today.AddDays(-1);
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Due date must be on or after invoice date");
    }

    [Fact]
    public void Empty_lines_fails()
    {
        var dto = ValidDto(); dto.Lines = new();
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invoice must have at least one line");
    }
}

public class CreateInvoiceLineValidatorTests
{
    private readonly CreateInvoiceLineValidator _validator = new();

    private CreateInvoiceLineDto ValidLine() => new()
    {
        Description = "Product A",
        Quantity = 3,
        UnitPrice = 150,
        DiscountPercent = 10,
        RevenueAccountId = Guid.NewGuid()
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
    public void Discount_over_100_fails()
    {
        var line = ValidLine(); line.DiscountPercent = 101;
        _validator.Validate(line).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_revenue_account_id_fails()
    {
        var line = ValidLine(); line.RevenueAccountId = Guid.Empty;
        var result = _validator.Validate(line);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Revenue account is required");
    }
}
