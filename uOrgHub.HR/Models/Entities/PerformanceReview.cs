using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_performance_reviews")]
public class PerformanceReview : BaseEntity
{
    public Guid ReviewCycleId { get; set; }
    public ReviewCycle ReviewCycle { get; set; } = null!;

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid ReviewerId { get; set; }
    public Employee Reviewer { get; set; } = null!;

    public PerformanceReviewType ReviewType { get; set; }
    [Column(TypeName = "decimal(3,1)")] public decimal? OverallRating { get; set; }
    [MaxLength(3000)] public string? Comments { get; set; }
    [MaxLength(3000)] public string? Strengths { get; set; }
    [MaxLength(3000)] public string? AreasForImprovement { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Draft;
    public DateTime DueDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? AcknowledgedDate { get; set; }

    public ICollection<FeedbackRequest> FeedbackRequests { get; set; } = new List<FeedbackRequest>();
}
