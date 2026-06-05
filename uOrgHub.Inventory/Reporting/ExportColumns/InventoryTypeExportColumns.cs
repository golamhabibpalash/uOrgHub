using uOrgHub.Inventory.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Inventory.Reporting.ExportColumns;

public static class InventoryTypeExportColumns
{
    public static List<ExportColumn<InventoryTypeResponseDto>> Get() =>
    [
        new("id", "Id", x => x.Id),
        new("name", "Name", x => x.Name),
        new("code", "Code", x => x.Code),
        new("description", "Description", x => x.Description),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
