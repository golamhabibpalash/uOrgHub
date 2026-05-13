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
    public partial void UpdateEntity(UpdateEmployeeDto dto, Employee entity);
}
