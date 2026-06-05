using uOrgHub.Accounts.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class AccountGroupExportColumns
{
    public static List<ExportColumn<AccountGroupResponseDto>> Get() =>
    [
        new("code", "Code", x => x.Code),
        new("name", "Name", x => x.Name),
        new("type", "Type", x => x.Type.ToString()),
        new("description", "Description", x => x.Description),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
        new("createdBy", "Created By", x => x.CreatedBy),
    ];
}
