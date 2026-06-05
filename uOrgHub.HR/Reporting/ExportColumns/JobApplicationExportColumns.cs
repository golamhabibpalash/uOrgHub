using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class JobApplicationExportColumns
{
    public static List<ExportColumn<JobApplicationResponseDto>> Get() =>
    [
        new("candidateName", "Candidate", x => x.CandidateName),
        new("candidateEmail", "Email", x => x.CandidateEmail),
        new("jobPostingTitle", "Job Posting", x => x.JobPostingTitle),
        new("applicationDate", "Application Date", x => x.ApplicationDate),
        new("status", "Status", x => x.Status.ToString()),
        new("hiringScore", "Hiring Score", x => x.HiringScore),
        new("notes", "Notes", x => x.Notes),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
