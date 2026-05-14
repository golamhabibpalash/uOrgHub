using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_project_milestones")]
public class ProjectMilestone : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(1000)]          public string? Description { get; set; }

    public DateTime PlannedDate { get; set; }
    public DateTime? ActualDate { get; set; }

    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
    public bool IsCritical { get; set; } = false;

    [MaxLength(500)] public string? Notes { get; set; }
}
