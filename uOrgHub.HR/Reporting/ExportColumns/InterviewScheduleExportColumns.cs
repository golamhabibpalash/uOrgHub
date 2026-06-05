using uOrgHub.HR.DTOs.Recruitment;
using uOrgHub.Shared.Export;

namespace uOrgHub.HR.Reporting.ExportColumns;

public static class InterviewScheduleExportColumns
{
    public static List<ExportColumn<InterviewScheduleResponseDto>> Get() =>
    [
        new("candidateName", "Candidate", x => x.CandidateName),
        new("interviewType", "Interview Type", x => x.InterviewType.ToString()),
        new("scheduledAt", "Scheduled At", x => x.ScheduledAt),
        new("durationMinutes", "Duration (min)", x => x.DurationMinutes),
        new("location", "Location", x => x.Location),
        new("meetingLink", "Meeting Link", x => x.MeetingLink),
        new("status", "Status", x => x.Status.ToString()),
        new("feedback", "Feedback", x => x.Feedback),
        new("rating", "Rating", x => x.Rating),
        new("createdAt", "Created At", x => x.CreatedAt),
    ];
}
