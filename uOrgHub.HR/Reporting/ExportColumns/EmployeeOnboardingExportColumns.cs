using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class EmployeeOnboardingExportColumns
{
    public static List<ExportColumn<EmployeeOnboardingResponseDto>> Get() =>
    [
        new("employeeName", "Employee", x => x.EmployeeName),
        new("checklistName", "Checklist", x => x.ChecklistName),
        new("startDate", "Start Date", x => x.StartDate),
        new("completionDate", "Completion Date", x => x.CompletionDate),
        new("status", "Status", x => x.Status.ToString()),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
