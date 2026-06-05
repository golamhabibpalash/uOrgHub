using uOrgHub.Inventory.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Inventory.Reporting.ExportColumns;

public static class AttributeDefinitionExportColumns
{
    public static List<ExportColumn<AttributeDefinitionResponseDto>> Get() =>
    [
        new("id", "Id", x => x.Id),
        new("name", "Name", x => x.Name),
        new("dataType", "Data Type", x => x.DataType.ToString()),
        new("categoryName", "Category", x => x.CategoryName),
        new("isRequired", "Required", x => x.IsRequired ? "Yes" : "No"),
        new("predefinedValues", "Predefined Values", x => x.PredefinedValues),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
