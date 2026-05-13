using Riok.Mapperly.Abstractions;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Mappings;

[Mapper]
public partial class DepartmentMapper
{
    public partial DepartmentResponseDto ToDto(Department entity);

    [MapperIgnoreTarget(nameof(Department.Id))]
    [MapperIgnoreTarget(nameof(Department.CreatedAt))]
    [MapperIgnoreTarget(nameof(Department.CreatedBy))]
    [MapperIgnoreTarget(nameof(Department.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Department.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Department.IsDeleted))]
    [MapperIgnoreTarget(nameof(Department.DeletedAt))]
    [MapperIgnoreTarget(nameof(Department.DeletedBy))]
    [MapperIgnoreTarget(nameof(Department.Designations))]
    [MapperIgnoreTarget(nameof(Department.Employees))]
    public partial Department ToEntity(CreateDepartmentDto dto);

    [MapperIgnoreTarget(nameof(Department.Id))]
    [MapperIgnoreTarget(nameof(Department.CreatedAt))]
    [MapperIgnoreTarget(nameof(Department.CreatedBy))]
    [MapperIgnoreTarget(nameof(Department.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Department.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Department.IsDeleted))]
    [MapperIgnoreTarget(nameof(Department.DeletedAt))]
    [MapperIgnoreTarget(nameof(Department.DeletedBy))]
    [MapperIgnoreTarget(nameof(Department.Designations))]
    [MapperIgnoreTarget(nameof(Department.Employees))]
    public partial void UpdateEntity(UpdateDepartmentDto dto, Department entity);
}
