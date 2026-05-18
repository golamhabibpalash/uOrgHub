using FluentAssertions;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.DTOs.Validators;
using uOrgHub.HR.Models.Enums;

namespace uOrgHub.Tests.HR.Validators;

public class CreateEmployeeDtoValidatorTests
{
    private readonly CreateEmployeeDtoValidator _validator = new();

    private CreateEmployeeDto ValidDto() => new()
    {
        EmployeeCode = "EMP001",
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com",
        DesignationId = Guid.NewGuid(),
        DepartmentId = Guid.NewGuid(),
        JoiningDate = DateTime.UtcNow,
        BasicSalary = 50000
    };

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.Validate(ValidDto()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_employee_code_fails()
    {
        var dto = ValidDto(); dto.EmployeeCode = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Employee_code_over_20_chars_fails()
    {
        var dto = ValidDto(); dto.EmployeeCode = new string('E', 21);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_first_name_fails()
    {
        var dto = ValidDto(); dto.FirstName = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void First_name_over_100_chars_fails()
    {
        var dto = ValidDto(); dto.FirstName = new string('A', 101);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_last_name_fails()
    {
        var dto = ValidDto(); dto.LastName = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Last_name_over_100_chars_fails()
    {
        var dto = ValidDto(); dto.LastName = new string('B', 101);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_email_fails()
    {
        var dto = ValidDto(); dto.Email = "";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Invalid_email_format_fails()
    {
        var dto = ValidDto(); dto.Email = "not-an-email";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Email_over_200_chars_fails()
    {
        var dto = ValidDto(); dto.Email = new string('a', 195) + "@x.com";
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_designation_id_fails()
    {
        var dto = ValidDto(); dto.DesignationId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_department_id_fails()
    {
        var dto = ValidDto(); dto.DepartmentId = Guid.Empty;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_basic_salary_fails()
    {
        var dto = ValidDto(); dto.BasicSalary = -1;
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_basic_salary_passes()
    {
        var dto = ValidDto(); dto.BasicSalary = 0;
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void NationalId_within_30_chars_passes()
    {
        var dto = ValidDto(); dto.NationalId = new string('1', 30);
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void NationalId_over_30_chars_fails()
    {
        var dto = ValidDto(); dto.NationalId = new string('1', 31);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Phone_within_20_chars_passes()
    {
        var dto = ValidDto(); dto.Phone = "01700000000";
        _validator.Validate(dto).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Phone_over_20_chars_fails()
    {
        var dto = ValidDto(); dto.Phone = new string('0', 21);
        _validator.Validate(dto).IsValid.Should().BeFalse();
    }
}
