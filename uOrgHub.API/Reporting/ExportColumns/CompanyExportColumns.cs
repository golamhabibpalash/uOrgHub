using uOrgHub.API.Controllers;
using uOrgHub.Shared.Export;

namespace uOrgHub.API.Reporting.ExportColumns;

public static class CompanyExportColumns
{
    public static List<ExportColumn<CompanyDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("tagLine", "Tag Line", x => x.TagLine),
        new("address", "Address", x => x.Address),
        new("phone", "Phone", x => x.Phone),
        new("email", "Email", x => x.Email),
        new("taxId", "Tax ID", x => x.TaxId),
        new("currency", "Currency", x => x.Currency),
        new("timeZone", "Time Zone", x => x.TimeZone),
        new("isActive", "Is Active", x => x.IsActive),
        new("createdAt", "Created At", x => x.CreatedAt),
        new("updatedAt", "Updated At", x => x.UpdatedAt),
    ];
}
