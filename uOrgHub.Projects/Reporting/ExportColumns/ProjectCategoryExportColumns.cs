using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class ProjectCategoryExportColumns
{
    public static List<ExportColumn<ProjectCategoryResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("code", "Code", x => x.Code),
        new("description", "Description", x => x.Description),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
