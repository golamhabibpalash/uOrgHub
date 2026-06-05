using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class PayrollCycleExportColumns
{
    public static List<ExportColumn<PayrollCycleResponseDto>> Get() =>
    [
        new("title", "Title", x => x.Title),
        new("year", "Year", x => x.Year),
        new("month", "Month", x => x.Month),
        new("startDate", "Start Date", x => x.StartDate),
        new("endDate", "End Date", x => x.EndDate),
        new("status", "Status", x => x.Status.ToString()),
        new("totalBasic", "Total Basic", x => x.TotalBasic),
        new("totalAllowances", "Total Allowances", x => x.TotalAllowances),
        new("totalDeductions", "Total Deductions", x => x.TotalDeductions),
        new("totalNetPay", "Total Net Pay", x => x.TotalNetPay),
        new("totalEmployees", "Total Employees", x => x.TotalEmployees),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
