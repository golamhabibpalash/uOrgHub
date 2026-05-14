using Riok.Mapperly.Abstractions;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Mappings;

[Mapper]
public partial class WarehouseMapper
{
    public partial WarehouseResponseDto ToDto(Warehouse entity);

    [MapperIgnoreTarget(nameof(Warehouse.Id))]
    [MapperIgnoreTarget(nameof(Warehouse.CreatedAt))]
    [MapperIgnoreTarget(nameof(Warehouse.CreatedBy))]
    [MapperIgnoreTarget(nameof(Warehouse.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Warehouse.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Warehouse.IsDeleted))]
    [MapperIgnoreTarget(nameof(Warehouse.DeletedAt))]
    [MapperIgnoreTarget(nameof(Warehouse.DeletedBy))]
    [MapperIgnoreTarget(nameof(Warehouse.IsActive))]
    public partial Warehouse ToEntity(CreateWarehouseDto dto);

    [MapperIgnoreTarget(nameof(Warehouse.Id))]
    [MapperIgnoreTarget(nameof(Warehouse.CreatedAt))]
    [MapperIgnoreTarget(nameof(Warehouse.CreatedBy))]
    [MapperIgnoreTarget(nameof(Warehouse.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Warehouse.UpdatedBy))]
    [MapperIgnoreTarget(nameof(Warehouse.IsDeleted))]
    [MapperIgnoreTarget(nameof(Warehouse.DeletedAt))]
    [MapperIgnoreTarget(nameof(Warehouse.DeletedBy))]
    public partial void UpdateEntity(UpdateWarehouseDto dto, Warehouse entity);
}
