using Riok.Mapperly.Abstractions;
using uOrgHub.Inventory.DTOs;
using uOrgHub.Inventory.Models.Entities;

namespace uOrgHub.Inventory.Mappings;

[Mapper]
public partial class InventoryTypeMapper
{
    public partial InventoryTypeResponseDto ToDto(InventoryType entity);

    [MapperIgnoreTarget(nameof(InventoryType.Id))]
    [MapperIgnoreTarget(nameof(InventoryType.CreatedAt))]
    [MapperIgnoreTarget(nameof(InventoryType.CreatedBy))]
    [MapperIgnoreTarget(nameof(InventoryType.UpdatedAt))]
    [MapperIgnoreTarget(nameof(InventoryType.UpdatedBy))]
    [MapperIgnoreTarget(nameof(InventoryType.IsDeleted))]
    [MapperIgnoreTarget(nameof(InventoryType.DeletedAt))]
    [MapperIgnoreTarget(nameof(InventoryType.DeletedBy))]
    [MapperIgnoreTarget(nameof(InventoryType.IsActive))]
    public partial InventoryType ToEntity(CreateInventoryTypeDto dto);

    [MapperIgnoreTarget(nameof(InventoryType.Id))]
    [MapperIgnoreTarget(nameof(InventoryType.CreatedAt))]
    [MapperIgnoreTarget(nameof(InventoryType.CreatedBy))]
    [MapperIgnoreTarget(nameof(InventoryType.UpdatedAt))]
    [MapperIgnoreTarget(nameof(InventoryType.UpdatedBy))]
    [MapperIgnoreTarget(nameof(InventoryType.IsDeleted))]
    [MapperIgnoreTarget(nameof(InventoryType.DeletedAt))]
    [MapperIgnoreTarget(nameof(InventoryType.DeletedBy))]
    public partial void UpdateEntity(UpdateInventoryTypeDto dto, InventoryType entity);
}
