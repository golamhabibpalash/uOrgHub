using uOrgHub.Inventory.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Inventory.Reporting.ExportColumns;

public static class UnitOfMeasureExportColumns
{
    public static List<ExportColumn<UnitOfMeasureResponseDto>> Get() =>
    [
        new("id", "Id", x => x.Id),
        new("name", "Name", x => x.Name),
        new("abbreviation", "Abbreviation", x => x.Abbreviation),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
