using Riok.Mapperly.Abstractions;
using uOrgHub.HR.DTOs;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Mappings;

[Mapper]
public partial class DesignationMapper
{
    public partial DesignationResponseDto ToDto(Designation entity);

    [MapperIgnoreTarget(nameof(Designation.Id))]
    [MapperIgnoreTarget(nameof(Designation.CreatedAt))]
    [MapperIgnoreTarget(nameof(Designation.CreatedBy))]
    [MapperIgnoreTarget(nameof(Designation.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Designation.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Designation.IsDeleted))]
    [MapperIgnoreTarget(nameof(Designation.DeletedAt))]
    [MapperIgnoreTarget(nameof(Designation.DeletedBy))]
    [MapperIgnoreTarget(nameof(Designation.Department))]
    [MapperIgnoreTarget(nameof(Designation.Employees))]
    public partial Designation ToEntity(CreateDesignationDto dto);

    [MapperIgnoreTarget(nameof(Designation.Id))]
    [MapperIgnoreTarget(nameof(Designation.CreatedAt))]
    [MapperIgnoreTarget(nameof(Designation.CreatedBy))]
    [MapperIgnoreTarget(nameof(Designation.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Designation.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Designation.IsDeleted))]
    [MapperIgnoreTarget(nameof(Designation.DeletedAt))]
    [MapperIgnoreTarget(nameof(Designation.DeletedBy))]
    [MapperIgnoreTarget(nameof(Designation.Department))]
    [MapperIgnoreTarget(nameof(Designation.Employees))]
    public partial void UpdateEntity(UpdateDesignationDto dto, Designation entity);
}
