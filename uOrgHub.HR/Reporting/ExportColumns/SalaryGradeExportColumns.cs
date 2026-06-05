using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class SalaryGradeExportColumns
{
    public static List<ExportColumn<SalaryGradeResponseDto>> Get() =>
    [
        new("gradeCode", "Grade Code", x => x.GradeCode),
        new("name", "Name", x => x.Name),
        new("minSalary", "Min Salary", x => x.MinSalary),
        new("maxSalary", "Max Salary", x => x.MaxSalary),
        new("description", "Description", x => x.Description),
        new("isActive", "Active", x => x.IsActive ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
