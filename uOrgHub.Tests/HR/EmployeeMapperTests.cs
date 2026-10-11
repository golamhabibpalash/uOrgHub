using FluentAssertions;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Mappings;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.Tests.HR;

/// <summary>
/// Salary belongs to the salary structure. The employee form no longer carries it, so creating or
/// editing an employee must never write BasicSalary / SalaryGradeId — an edit used to be able to
/// reset them to 0 / null.
/// </summary>
public class EmployeeMapperTests
{
    private readonly EmployeeMapper _mapper = new();

    [Fact]
    public void Editing_an_employee_keeps_salary_set_by_the_structure()
    {
        var gradeId = Guid.NewGuid();
        var employee = new Employee { FirstName = "Old", LastName = "Name", BasicSalary = 30000, SalaryGradeId = gradeId };

        _mapper.UpdateEntity(new UpdateEmployeeDto { FirstName = "New", LastName = "Name", Email = "n@test.local" }, employee);

        employee.FirstName.Should().Be("New");
        employee.BasicSalary.Should().Be(30000);
        employee.SalaryGradeId.Should().Be(gradeId);
    }

    [Fact]
    public void New_employee_starts_without_salary()
    {
        var employee = _mapper.ToEntity(new CreateEmployeeDto { FirstName = "A", LastName = "B", Email = "a@test.local" });

        employee.BasicSalary.Should().Be(0);
        employee.SalaryGradeId.Should().BeNull();
    }
}
