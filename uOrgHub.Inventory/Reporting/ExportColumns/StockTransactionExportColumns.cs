using uOrgHub.Inventory.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Inventory.Reporting.ExportColumns;

public static class StockTransactionExportColumns
{
    public static List<ExportColumn<StockTransactionResponseDto>> Get() =>
    [
        new("id", "Id", x => x.Id),
        new("transactionNumber", "Transaction #", x => x.TransactionNumber),
        new("transactionDate", "Date", x => x.TransactionDate),
        new("transactionType", "Type", x => x.TransactionTypeName),
        new("status", "Status", x => x.StatusName),
        new("variantSKU", "Variant SKU", x => x.VariantSKU),
        new("variantName", "Variant Name", x => x.VariantName),
        new("warehouseName", "Warehouse", x => x.WarehouseName),
        new("fromWarehouseName", "From Warehouse", x => x.FromWarehouseName),
        new("quantity", "Quantity", x => x.Quantity),
        new("unitCost", "Unit Cost", x => x.UnitCost),
        new("totalCost", "Total Cost", x => x.TotalCost),
        new("referenceNumber", "Reference", x => x.ReferenceNumber),
        new("notes", "Notes", x => x.Notes),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
