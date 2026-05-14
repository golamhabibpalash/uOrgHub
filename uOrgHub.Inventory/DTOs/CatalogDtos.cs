using uOrgHub.Inventory.Models.Enums;

namespace uOrgHub.Inventory.DTOs;

// InventoryType
public record CreateInventoryTypeDto(string Name, string Code, string? Description);
public record UpdateInventoryTypeDto(string Name, string Code, string? Description, bool IsActive);
public class InventoryTypeResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// InventoryCategory
public record CreateInventoryCategoryDto(string Name, string Code, Guid TypeId, Guid? ParentCategoryId, string? Description);
public record UpdateInventoryCategoryDto(string Name, string Code, Guid TypeId, Guid? ParentCategoryId, string? Description, bool IsActive);
public class InventoryCategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// UnitOfMeasure
public record CreateUnitOfMeasureDto(string Name, string Abbreviation);
public record UpdateUnitOfMeasureDto(string Name, string Abbreviation, bool IsActive);
public class UnitOfMeasureResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// AttributeDefinition
public record CreateAttributeDefinitionDto(string Name, AttributeDataType DataType, Guid? CategoryId, bool IsRequired, string? PredefinedValues);
public record UpdateAttributeDefinitionDto(string Name, AttributeDataType DataType, Guid? CategoryId, bool IsRequired, string? PredefinedValues, bool IsActive);
public class AttributeDefinitionResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AttributeDataType DataType { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsRequired { get; set; }
    public string? PredefinedValues { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
