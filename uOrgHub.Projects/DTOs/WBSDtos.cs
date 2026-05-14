using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateWBSDto
{
    public Guid ProjectId { get; set; }
    public Guid? ParentWBSId { get; set; }
    public string WBSCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public int PlannedDuration { get; set; }
    public WBSStatus Status { get; set; } = WBSStatus.NotStarted;
    public int Sequence { get; set; }
}

public class UpdateWBSDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int PlannedDuration { get; set; }
    public WBSStatus Status { get; set; }
    public decimal CompletionPercent { get; set; }
    public int Sequence { get; set; }
}

public class WBSResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? ParentWBSId { get; set; }
    public string WBSCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int PlannedDuration { get; set; }
    public WBSStatus Status { get; set; }
    public decimal CompletionPercent { get; set; }
    public int Sequence { get; set; }
    public List<WBSResponseDto> Children { get; set; } = new();
}
