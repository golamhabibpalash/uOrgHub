namespace uOrgHub.Inventory.DTOs;

// Item
public record CreateItemDto(
    string BaseName,
    Guid TypeId,
    Guid CategoryId,
    Guid UnitOfMeasureId,
    string? Brand,
    string? Manufacturer,
    string? Description,
    decimal ReorderLevel,
    decimal StandardCost);

public record UpdateItemDto(
    string BaseName,
    Guid TypeId,
    Guid CategoryId,
    Guid UnitOfMeasureId,
    string? Brand,
    string? Manufacturer,
    string? Description,
    decimal ReorderLevel,
    decimal StandardCost,
    bool IsActive);

public class ItemResponseDto
{
    public Guid Id { get; set; }
    public string BaseName { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public Guid TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid UnitOfMeasureId { get; set; }
    public string UnitOfMeasureName { get; set; } = string.Empty;
    public string UnitAbbreviation { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal StandardCost { get; set; }
    public bool IsActive { get; set; }
    public int VariantCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ItemVariant
public record CreateItemVariantDto(
    Guid ItemId,
    string? Barcode,
    decimal CostPrice,
    decimal SellingPrice,
    bool IsDefault,
    List<VariantAttributeValueDto> Attributes);

public record UpdateItemVariantDto(
    string? Barcode,
    decimal CostPrice,
    decimal SellingPrice,
    bool IsDefault,
    bool IsActive,
    List<VariantAttributeValueDto> Attributes);

public record VariantAttributeValueDto(Guid AttributeDefinitionId, string Value);

public class ItemVariantResponseDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string ItemBaseName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public string? AttributeHash { get; set; }
    public List<VariantAttributeResponseDto> Attributes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class VariantAttributeResponseDto
{
    public Guid Id { get; set; }
    public Guid AttributeDefinitionId { get; set; }
    public string AttributeName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
