using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class PayrollEntryExportColumns
{
    public static List<ExportColumn<PayrollEntryResponseDto>> Get() =>
    [
        new("employeeCode", "Employee Code", x => x.EmployeeCode),
        new("employeeName", "Employee", x => x.EmployeeName),
        new("grossSalary", "Gross Salary", x => x.GrossSalary),
        new("basicSalary", "Basic Salary", x => x.BasicSalary),
        new("totalAllowances", "Allowances", x => x.TotalAllowances),
        new("totalDeductions", "Deductions", x => x.TotalDeductions),
        new("taxAmount", "Tax", x => x.TaxAmount),
        new("netSalary", "Net Salary", x => x.NetSalary),
        new("overtimePay", "Overtime Pay", x => x.OvertimePay),
        new("bonusAmount", "Bonus", x => x.BonusAmount),
        new("totalWorkingDays", "Working Days", x => x.TotalWorkingDays),
        new("presentDays", "Present", x => x.PresentDays),
        new("absentDays", "Absent", x => x.AbsentDays),
        new("leaveDays", "Leave Days", x => x.LeaveDays),
        new("status", "Status", x => x.Status.ToString()),
    ];
}
