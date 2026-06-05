using uOrgHub.HR.DTOs.Performance;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class TrainingProgramExportColumns
{
    public static List<ExportColumn<TrainingProgramResponseDto>> Get() =>
    [
        new("title", "Title", x => x.Title),
        new("description", "Description", x => x.Description),
        new("category", "Category", x => x.Category),
        new("mode", "Mode", x => x.Mode.ToString()),
        new("durationHours", "Duration (hrs)", x => x.DurationHours),
        new("provider", "Provider", x => x.Provider),
        new("location", "Location", x => x.Location),
        new("cost", "Cost", x => x.Cost),
        new("startDate", "Start Date", x => x.StartDate),
        new("endDate", "End Date", x => x.EndDate),
        new("status", "Status", x => x.Status.ToString()),
        new("hasCertificate", "Certificate", x => x.HasCertificate ? "Yes" : "No"),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
