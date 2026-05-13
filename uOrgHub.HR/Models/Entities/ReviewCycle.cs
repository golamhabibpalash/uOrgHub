using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_review_cycles")]
public class ReviewCycle : BaseEntity
{
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    public ReviewCycleType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Draft;
    [MaxLength(500)] public string? Description { get; set; }

    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
