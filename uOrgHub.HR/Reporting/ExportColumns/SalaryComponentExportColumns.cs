using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class SalaryComponentExportColumns
{
    public static List<ExportColumn<SalaryComponentResponseDto>> Get() =>
    [
        new("name", "Name", x => x.Name),
        new("code", "Code", x => x.Code),
        new("componentType", "Type", x => x.ComponentType.ToString()),
        new("calculationType", "Calculation", x => x.CalculationType.ToString()),
        new("defaultValue", "Default Value", x => x.DefaultValue),
        new("isTaxable", "Taxable", x => x.IsTaxable ? "Yes" : "No"),
        new("isFixed", "Fixed", x => x.IsFixed ? "Yes" : "No"),
        new("sortOrder", "Sort Order", x => x.SortOrder),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
