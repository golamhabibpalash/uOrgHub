using uOrgHub.Auth.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Auth.Reporting.ExportColumns;

public static class RoleExportColumns
{
    public static List<ExportColumn<RoleDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("description", "Description", x => x.Description),
        new("isSystem", "Is System", x => x.IsSystem),
        new("isActive", "Is Active", x => x.IsActive),
        new("userCount", "User Count", x => x.UserCount),
    ];
}
