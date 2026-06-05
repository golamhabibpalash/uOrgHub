using uOrgHub.Inventory.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Inventory.Reporting.ExportColumns;

public static class InventoryCategoryExportColumns
{
    public static List<ExportColumn<InventoryCategoryResponseDto>> Get() =>
    [
        new("id", "Id", x => x.Id),
        new("name", "Name", x => x.Name),
        new("code", "Code", x => x.Code),
        new("typeName", "Type", x => x.TypeName),
        new("parentCategoryName", "Parent Category", x => x.ParentCategoryName),
        new("description", "Description", x => x.Description),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
