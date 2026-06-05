using uOrgHub.Inventory.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Inventory.Reporting.ExportColumns;

public static class StockBalanceExportColumns
{
    public static List<ExportColumn<StockBalanceResponseDto>> Get() =>
    [
        new("id", "Id", x => x.Id),
        new("variantSKU", "Variant SKU", x => x.VariantSKU),
        new("variantName", "Variant Name", x => x.VariantName),
        new("itemBaseName", "Item Name", x => x.ItemBaseName),
        new("warehouseName", "Warehouse", x => x.WarehouseName),
        new("warehouseCode", "Warehouse Code", x => x.WarehouseCode),
        new("quantityOnHand", "On Hand", x => x.QuantityOnHand),
        new("quantityReserved", "Reserved", x => x.QuantityReserved),
        new("quantityAvailable", "Available", x => x.QuantityAvailable),
        new("lastUpdated", "Last Updated", x => x.LastUpdated),
    ];
}
