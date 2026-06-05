using uOrgHub.HR.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class EmployeeExportColumns
{
    public static List<ExportColumn<EmployeeResponseDto>> Get() =>
    [
        new("employeeCode", "Employee Code", x => x.EmployeeCode),
        new("firstName", "First Name", x => x.FirstName),
        new("lastName", "Last Name", x => x.LastName),
        new("email", "Email", x => x.Email),
        new("phone", "Phone", x => x.Phone),
        new("departmentName", "Department", x => x.DepartmentName),
        new("designationName", "Designation", x => x.DesignationName),
        new("employmentType", "Employment Type", x => x.EmploymentType.ToString()),
        new("status", "Status", x => x.Status.ToString()),
        new("joiningDate", "Joining Date", x => x.JoiningDate),
        new("basicSalary", "Basic Salary", x => x.BasicSalary),
        new("nationality", "Nationality", x => x.Nationality),
    ];
}
