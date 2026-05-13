using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_interview_schedules")]
public class InterviewSchedule : BaseEntity
{
    public Guid JobApplicationId { get; set; }
    public JobApplication JobApplication { get; set; } = null!;

    public InterviewType InterviewType { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 60;
    [MaxLength(500)] public string? Location { get; set; }
    [MaxLength(500)] public string? MeetingLink { get; set; }
    [MaxLength(500)] public string? InterviewerIds { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    [MaxLength(2000)] public string? Feedback { get; set; }
    public int? Rating { get; set; }
    [MaxLength(500)] public string? CancellationReason { get; set; }
}
