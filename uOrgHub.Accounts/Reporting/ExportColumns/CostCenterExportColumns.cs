using uOrgHub.Accounts.DTOs.CostCenter;
using uOrgHub.Shared.Export;

namespace uOrgHub.Accounts.Reporting.ExportColumns;

public static class CostCenterExportColumns
{
    public static List<ExportColumn<CostCenterResponseDto>> Get() =>
    [
        new("code", "Code", x => x.Code),
        new("name", "Name", x => x.Name),
        new("description", "Description", x => x.Description),
        new("parentCostCenter", "Parent Cost Center", x => x.ParentCostCenterName),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
    ];
}
