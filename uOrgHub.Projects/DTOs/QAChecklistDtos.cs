using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateQAChecklistDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public InspectionType InspectionType { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public Guid? InspectedById { get; set; }
    public List<CreateQAChecklistItemDto> Items { get; set; } = new();
}

public class UpdateQAChecklistDto
{
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public InspectionType InspectionType { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public Guid? InspectedById { get; set; }
    public string? Remarks { get; set; }
}

public class SubmitQAChecklistDto
{
    public Guid InspectedById { get; set; }
    public DateTime InspectedDate { get; set; }
    public InspectionResult OverallResult { get; set; }
    public string? Remarks { get; set; }
    public List<UpdateQAChecklistItemDto> Items { get; set; } = new();
}

public class CreateQAChecklistItemDto
{
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public int Sequence { get; set; }
}

public class UpdateQAChecklistItemDto
{
    public Guid Id { get; set; }
    public InspectionResult Result { get; set; }
    public string? Remarks { get; set; }
}

public class QAChecklistResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string ChecklistNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public InspectionType InspectionType { get; set; }
    public QAChecklistStatus Status { get; set; }
    public Guid? InspectedById { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? InspectedDate { get; set; }
    public InspectionResult? OverallResult { get; set; }
    public string? Remarks { get; set; }
    public List<QAChecklistItemResponseDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class QAChecklistItemResponseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int Sequence { get; set; }
    public InspectionResult? Result { get; set; }
    public string? Remarks { get; set; }
}
