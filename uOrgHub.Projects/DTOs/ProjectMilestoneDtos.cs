using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateProjectMilestoneDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
    public bool IsCritical { get; set; } = false;
    public string? Notes { get; set; }
}

public class UpdateProjectMilestoneDto
{
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public MilestoneStatus Status { get; set; }
    public bool IsCritical { get; set; }
    public string? Notes { get; set; }
}

public class ProjectMilestoneResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PlannedDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public MilestoneStatus Status { get; set; }
    public bool IsCritical { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
