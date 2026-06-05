using uOrgHub.Inventory.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Inventory.Reporting.ExportColumns;

public static class ItemVariantExportColumns
{
    public static List<ExportColumn<ItemVariantResponseDto>> Get() =>
    [
        new("id", "Id", x => x.Id),
        new("itemBaseName", "Item Name", x => x.ItemBaseName),
        new("sku", "SKU", x => x.SKU),
        new("variantName", "Variant Name", x => x.VariantName),
        new("barcode", "Barcode", x => x.Barcode),
        new("costPrice", "Cost Price", x => x.CostPrice),
        new("sellingPrice", "Selling Price", x => x.SellingPrice),
        new("isDefault", "Default", x => x.IsDefault ? "Yes" : "No"),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("attributeHash", "Attribute Hash", x => x.AttributeHash),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
