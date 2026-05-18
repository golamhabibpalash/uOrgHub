using FluentAssertions;
using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.HR.DTOs.Validators;

namespace uOrgHub.Tests.HR.Validators;

public class CreateSalaryGradeDtoValidatorTests
{
    private readonly CreateSalaryGradeDtoValidator _validator = new();

    private CreateSalaryGradeDto ValidDto() => new()
    {
        GradeCode = "G1",
        Name = "Grade 1",
        MinSalary = 20000,
        MaxSalary = 50000
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_grade_code_fails()
    {
        var dto = ValidDto(); dto.GradeCode = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_name_fails()
    {
        var dto = ValidDto(); dto.Name = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_min_salary_fails()
    {
        var dto = ValidDto(); dto.MinSalary = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_min_salary_passes()
    {
        var dto = ValidDto(); dto.MinSalary = 0;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Max_salary_equal_to_min_salary_fails()
    {
        var dto = ValidDto(); dto.MinSalary = 50000; dto.MaxSalary = 50000;
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Max salary must be greater than min salary"));
    }

    [Fact]
    public void Max_salary_less_than_min_salary_fails()
    {
        var dto = ValidDto(); dto.MinSalary = 60000; dto.MaxSalary = 50000;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}

public class CreateSalaryComponentDtoValidatorTests
{
    private readonly CreateSalaryComponentDtoValidator _validator = new();

    private CreateSalaryComponentDto ValidDto() => new()
    {
        Name = "Basic Salary",
        Code = "BASIC",
        DefaultValue = 10000
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
    public void Empty_code_fails()
    {
        var dto = ValidDto(); dto.Code = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_default_value_fails()
    {
        var dto = ValidDto(); dto.DefaultValue = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_default_value_passes()
    {
        var dto = ValidDto(); dto.DefaultValue = 0;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }
}

public class CreatePayrollCycleDtoValidatorTests
{
    private readonly CreatePayrollCycleDtoValidator _validator = new();

    private CreatePayrollCycleDto ValidDto() => new()
    {
        Year = 2026,
        Month = 5,
        Title = "May 2026 Payroll",
        StartDate = new DateTime(2026, 5, 1),
        EndDate = new DateTime(2026, 5, 31)
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Year_below_2000_fails()
    {
        var dto = ValidDto(); dto.Year = 1999;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Year_2100_passes()
    {
        var dto = ValidDto(); dto.Year = 2100;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Year_above_2100_fails()
    {
        var dto = ValidDto(); dto.Year = 2101;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Month_zero_fails()
    {
        var dto = ValidDto(); dto.Month = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Month_13_fails()
    {
        var dto = ValidDto(); dto.Month = 13;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Month_12_passes()
    {
        var dto = ValidDto(); dto.Month = 12;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_title_fails()
    {
        var dto = ValidDto(); dto.Title = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void End_date_before_start_date_fails()
    {
        var dto = ValidDto();
        dto.StartDate = new DateTime(2026, 5, 31);
        dto.EndDate = new DateTime(2026, 5, 1);
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("End date must be after start date"));
    }
}

public class CreateExpenseRequestDtoValidatorTests
{
    private readonly CreateExpenseRequestDtoValidator _validator = new();

    private CreateExpenseRequestDto ValidDto() => new()
    {
        EmployeeId = Guid.NewGuid(),
        Amount = 500,
        ExpenseDate = DateTime.Today,
        Description = "Travel to client site"
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_employee_id_fails()
    {
        var dto = ValidDto(); dto.EmployeeId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_amount_fails()
    {
        var dto = ValidDto(); dto.Amount = 0;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_description_fails()
    {
        var dto = ValidDto(); dto.Description = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Description_over_500_chars_fails()
    {
        var dto = ValidDto(); dto.Description = new string('D', 501);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
