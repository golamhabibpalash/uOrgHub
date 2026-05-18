using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateDrawingDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string DrawingNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Revision { get; set; } = "A";
    public DrawingDiscipline Discipline { get; set; }
    public Guid? DrawnById { get; set; }
    public Guid? CheckedById { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
}

public class UpdateDrawingDto
{
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public DrawingDiscipline Discipline { get; set; }
    public DrawingStatus Status { get; set; }
    public Guid? DrawnById { get; set; }
    public Guid? CheckedById { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? IssuedDate { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
}

public class DrawingResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string DrawingNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public DrawingDiscipline Discipline { get; set; }
    public DrawingStatus Status { get; set; }
    public Guid? DrawnById { get; set; }
    public Guid? CheckedById { get; set; }
    public Guid? ApprovedById { get; set; }
    public DateTime? IssuedDate { get; set; }
    public string? FilePath { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
