using uOrgHub.HR.DTOs.Performance;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class PerformanceReviewExportColumns
{
    public static List<ExportColumn<PerformanceReviewResponseDto>> Get() =>
    [
        new("employeeName", "Employee", x => x.EmployeeName),
        new("reviewCycleName", "Review Cycle", x => x.ReviewCycleName),
        new("reviewerName", "Reviewer", x => x.ReviewerName),
        new("reviewType", "Review Type", x => x.ReviewType.ToString()),
        new("overallRating", "Rating", x => x.OverallRating),
        new("status", "Status", x => x.Status.ToString()),
        new("dueDate", "Due Date", x => x.DueDate),
        new("submittedDate", "Submitted Date", x => x.SubmittedDate),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
