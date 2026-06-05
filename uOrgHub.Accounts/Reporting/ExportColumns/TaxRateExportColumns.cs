using uOrgHub.Accounts.DTOs.TaxRate;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class TaxRateExportColumns
{
    public static List<ExportColumn<TaxRateResponseDto>> Get() =>
    [
        new("code", "Code", x => x.Code),
        new("name", "Name", x => x.Name),
        new("taxType", "Tax Type", x => x.TaxType.ToString()),
        new("rate", "Rate (%)", x => x.Rate),
        new("description", "Description", x => x.Description),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
    ];
}
