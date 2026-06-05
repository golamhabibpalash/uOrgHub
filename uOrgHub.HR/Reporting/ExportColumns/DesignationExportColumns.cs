using uOrgHub.HR.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class DesignationExportColumns
{
    public static List<ExportColumn<DesignationResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("code", "Code", x => x.Code),
        new("level", "Level", x => x.Level),
        new("departmentName", "Department", x => x.DepartmentName),
        new("parentDesignationName", "Parent Designation", x => x.ParentDesignationName),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
