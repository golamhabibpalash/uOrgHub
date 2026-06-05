using uOrgHub.Inventory.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Inventory.Reporting.ExportColumns;

public static class WarehouseExportColumns
{
    public static List<ExportColumn<WarehouseResponseDto>> Get() =>
    [
        new("id", "Id", x => x.Id),
        new("name", "Name", x => x.Name),
        new("code", "Code", x => x.Code),
        new("location", "Location", x => x.Location),
        new("contactPerson", "Contact Person", x => x.ContactPerson),
        new("contactPhone", "Contact Phone", x => x.ContactPhone),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
