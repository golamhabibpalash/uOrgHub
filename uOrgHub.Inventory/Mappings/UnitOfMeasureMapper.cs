using Riok.Mapperly.Abstractions;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Mappings;

[Mapper]
public partial class UnitOfMeasureMapper
{
    public partial UnitOfMeasureResponseDto ToDto(UnitOfMeasure entity);

    [MapperIgnoreTarget(nameof(UnitOfMeasure.Id))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.CreatedAt))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.CreatedBy))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.UpdatedAt))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.UpdatedBy))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.IsDeleted))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.DeletedAt))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.DeletedBy))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.IsActive))]
    public partial UnitOfMeasure ToEntity(CreateUnitOfMeasureDto dto);

    [MapperIgnoreTarget(nameof(UnitOfMeasure.Id))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.CreatedAt))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.CreatedBy))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.UpdatedAt))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.UpdatedBy))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.IsDeleted))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.DeletedAt))]
    [MapperIgnoreTarget(nameof(UnitOfMeasure.DeletedBy))]
    public partial void UpdateEntity(UpdateUnitOfMeasureDto dto, UnitOfMeasure entity);
}
