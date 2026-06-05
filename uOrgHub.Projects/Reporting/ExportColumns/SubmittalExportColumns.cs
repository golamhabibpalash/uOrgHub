using uOrgHub.Projects.DTOs;
using uOrgHub.Shared.Export;

namespace uOrgHub.Projects.Reporting.ExportColumns;

public static class SubmittalExportColumns
{
    public static List<ExportColumn<SubmittalResponseDto>> Get() =>
    [
        new("submittalNumber", "Submittal Number", x => x.SubmittalNumber),
        new("title", "Title", x => x.Title),
        new("contractorReference", "Contractor Ref", x => x.ContractorReference),
        new("description", "Description", x => x.Description),
        new("status", "Status", x => x.Status.ToString()),
        new("submittedDate", "Submitted Date", x => x.SubmittedDate),
        new("reviewDate", "Review Date", x => x.ReviewDate),
        new("reviewComments", "Review Comments", x => x.ReviewComments),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
