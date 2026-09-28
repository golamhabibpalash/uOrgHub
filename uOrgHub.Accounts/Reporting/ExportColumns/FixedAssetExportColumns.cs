using uOrgHub.Accounts.DTOs.FixedAssets;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class FixedAssetExportColumns
{
    public static List<ExportColumn<FixedAssetResponseDto>> Get() =>
    [
        new("assetCode", "Asset Code", x => x.AssetCode),
        new("name", "Name", x => x.Name),
        new("category", "Category", x => x.CategoryName),
        new("manufacturer", "Manufacturer", x => x.Manufacturer),
        new("model", "Model", x => x.Model),
        new("serialNumber", "Serial No.", x => x.SerialNumber),
        new("chassisNumber", "Chassis No.", x => x.ChassisNumber),
        new("engineNumber", "Engine No.", x => x.EngineNumber),
        new("registrationNumber", "Registration No.", x => x.RegistrationNumber),
        new("purchaseDate", "Purchase Date", x => x.PurchaseDate.ToString("yyyy-MM-dd")),
        new("depreciationStartDate", "Depreciation Start", x => x.DepreciationStartDate.ToString("yyyy-MM-dd")),
        new("purchaseCost", "Cost", x => x.PurchaseCost),
        new("salvageValue", "Salvage Value", x => x.SalvageValue),
        new("usefulLifeMonths", "Useful Life (months)", x => x.UsefulLifeMonths),
        new("depreciationMethod", "Method", x => x.DepreciationMethod.ToString()),
        new("accumulatedDepreciation", "Accumulated Depreciation", x => x.AccumulatedDepreciation),
        new("bookValue", "Book Value", x => x.BookValue),
        new("lastDepreciationDate", "Depreciated Up To", x => x.LastDepreciationDate.HasValue ? x.LastDepreciationDate.Value.ToString("yyyy-MM-dd") : null),
        new("vendor", "Vendor", x => x.VendorName),
        new("bill", "Bill", x => x.BillBillNumber),
        new("costCenter", "Cost Center", x => x.CostCenterName),
        new("location", "Location", x => x.Location),
        new("status", "Status", x => x.Status.ToString()),
    ];
}

public static class DepreciationRunExportColumns
{
    public static List<ExportColumn<DepreciationRunResponseDto>> Get() =>
    [
        new("runNumber", "Run No.", x => x.RunNumber),
        new("period", "Period", x => $"{x.PeriodYear}-{x.PeriodMonth:D2}"),
        new("totalAmount", "Total Depreciation", x => x.TotalAmount),
        new("assetCount", "Assets", x => x.AssetCount),
        new("status", "Status", x => x.Status.ToString()),
        new("journalEntry", "Journal Entry", x => x.JournalEntryEntryNumber),
        new("createdBy", "Posted By", x => x.CreatedBy),
        new("createdAt", "Posted At", x => x.CreatedAt.ToString("yyyy-MM-dd HH:mm")),
        new("reversedBy", "Reversed By", x => x.ReversedBy),
        new("notes", "Notes", x => x.Notes),
    ];
}
