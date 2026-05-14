using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_daily_progress_reports")]
public class DailyProgressReport : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    public DateTime ReportDate { get; set; }
    public Guid ReportedById { get; set; }

    public WeatherCondition WeatherCondition { get; set; }

    [Required][MaxLength(2000)] public string WorkDone { get; set; } = string.Empty;
    [MaxLength(1000)]           public string? Issues { get; set; }
    [MaxLength(1000)]           public string? NextDayPlan { get; set; }

    public int ManpowerCount { get; set; }
    [MaxLength(500)] public string? EquipmentUsed { get; set; }

    public DPRStatus Status { get; set; } = DPRStatus.Draft;
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
