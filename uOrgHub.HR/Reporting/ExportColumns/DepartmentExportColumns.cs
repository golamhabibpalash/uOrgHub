using uOrgHub.HR.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class DepartmentExportColumns
{
    public static List<ExportColumn<DepartmentResponseDto>> Get() =>
    [
        new("name", "Department Name", x => x.Name),
        new("code", "Code", x => x.Code),
        new("parentDepartmentName", "Parent Department", x => x.ParentDepartmentName),
        new("description", "Description", x => x.Description),
        new("type", "Type", x => x.Type.ToString()),
        new("isActive", "Status", x => x.IsActive ? "Active" : "Inactive"),
        new("headOfDepartmentName", "Head of Department", x => x.HeadOfDepartmentName),
    ];
}
