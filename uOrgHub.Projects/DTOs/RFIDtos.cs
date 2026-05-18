using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateRFIDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid RaisedById { get; set; }
    public DateTime RaisedDate { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? ResponseDueDate { get; set; }
    public bool IsUrgent { get; set; } = false;
    public string? Notes { get; set; }
}

public class UpdateRFIDto
{
    public Guid? WBSId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? ResponseDueDate { get; set; }
    public bool IsUrgent { get; set; }
    public string? Notes { get; set; }
}

public class RespondRFIDto
{
    public string Response { get; set; } = string.Empty;
    public DateTime ResponseDate { get; set; }
}

public class RFIResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string RFINumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid RaisedById { get; set; }
    public DateTime RaisedDate { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? Response { get; set; }
    public RFIStatus Status { get; set; }
    public bool IsUrgent { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
