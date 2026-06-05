using uOrgHub.HR.DTOs.Performance;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class EmployeeTrainingExportColumns
{
    public static List<ExportColumn<EmployeeTrainingResponseDto>> Get() =>
    [
        new("employeeName", "Employee", x => x.EmployeeName),
        new("trainingTitle", "Training", x => x.TrainingTitle),
        new("enrollmentDate", "Enrollment Date", x => x.EnrollmentDate),
        new("completionDate", "Completion Date", x => x.CompletionDate),
        new("status", "Status", x => x.Status.ToString()),
        new("score", "Score", x => x.Score),
        new("remarks", "Remarks", x => x.Remarks),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
