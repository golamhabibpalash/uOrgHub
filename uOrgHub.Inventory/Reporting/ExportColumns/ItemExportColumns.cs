using uOrgHub.Inventory.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Inventory.Reporting.ExportColumns;

public static class ItemExportColumns
{
    public static List<ExportColumn<ItemResponseDto>> Get() =>
    [
        new("itemCode", "Item Code", x => x.ItemCode),
        new("baseName", "Item Name", x => x.BaseName),
        new("typeName", "Type", x => x.TypeName),
        new("categoryName", "Category", x => x.CategoryName),
        new("unitOfMeasureName", "UoM", x => x.UnitOfMeasureName),
        new("brand", "Brand", x => x.Brand),
        new("manufacturer", "Manufacturer", x => x.Manufacturer),
        new("standardCost", "Standard Cost", x => x.StandardCost),
        new("reorderLevel", "Reorder Level", x => x.ReorderLevel),
        new("variantCount", "Variants", x => x.VariantCount),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
    ];
}
