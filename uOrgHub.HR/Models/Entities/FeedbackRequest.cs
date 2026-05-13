using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_feedback_requests")]
public class FeedbackRequest : BaseEntity
{
    public Guid PerformanceReviewId { get; set; }
    public PerformanceReview PerformanceReview { get; set; } = null!;

    public Guid RequestedFromId { get; set; }
    public Employee RequestedFrom { get; set; } = null!;

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    [Column(TypeName = "decimal(3,1)")] public decimal? Rating { get; set; }
    [MaxLength(3000)] public string? FeedbackText { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime DueDate { get; set; }
}
