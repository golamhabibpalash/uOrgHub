using Riok.Mapperly.Abstractions;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Mappings;

[Mapper]
public partial class EmployeeMapper
{
    public partial EmployeeResponseDto ToDto(Employee entity);

    [MapperIgnoreTarget(nameof(Employee.Id))]
    [MapperIgnoreTarget(nameof(Employee.CreatedAt))]
    [MapperIgnoreTarget(nameof(Employee.CreatedBy))]
    [MapperIgnoreTarget(nameof(Employee.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Employee.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Employee.IsDeleted))]
    [MapperIgnoreTarget(nameof(Employee.DeletedAt))]
    [MapperIgnoreTarget(nameof(Employee.DeletedBy))]
    [MapperIgnoreTarget(nameof(Employee.Designation))]
    [MapperIgnoreTarget(nameof(Employee.Department))]
    // Salary is set only through the employee's salary structure, never the employee form.
    [MapperIgnoreTarget(nameof(Employee.BasicSalary))]
    [MapperIgnoreTarget(nameof(Employee.SalaryGradeId))]
    public partial Employee ToEntity(CreateEmployeeDto dto);

    [MapperIgnoreTarget(nameof(Employee.Id))]
    [MapperIgnoreTarget(nameof(Employee.CreatedAt))]
    [MapperIgnoreTarget(nameof(Employee.CreatedBy))]
    [MapperIgnoreTarget(nameof(Employee.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Employee.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Employee.IsDeleted))]
    [MapperIgnoreTarget(nameof(Employee.DeletedAt))]
    [MapperIgnoreTarget(nameof(Employee.DeletedBy))]
    [MapperIgnoreTarget(nameof(Employee.Designation))]
    [MapperIgnoreTarget(nameof(Employee.Department))]
    // Status is applied by the handler so an omitted value preserves the stored status.
    [MapperIgnoreTarget(nameof(Employee.Status))]
    [MapperIgnoreSource(nameof(UpdateEmployeeDto.Status))]
    // Salary is set only through the employee's salary structure; an edit must leave it untouched.
    [MapperIgnoreTarget(nameof(Employee.BasicSalary))]
    [MapperIgnoreTarget(nameof(Employee.SalaryGradeId))]
    public partial void UpdateEntity(UpdateEmployeeDto dto, Employee entity);
}
