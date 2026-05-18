using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateNCRDto
{
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NCRCategory Category { get; set; }
    public NCRSeverity Severity { get; set; }
    public Guid RaisedById { get; set; }
    public DateTime RaisedDate { get; set; }
    public string? ResponsibleParty { get; set; }
    public string? Notes { get; set; }
}

public class UpdateNCRDto
{
    public Guid? WBSId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NCRCategory Category { get; set; }
    public NCRSeverity Severity { get; set; }
    public string? ResponsibleParty { get; set; }
    public string? CorrectiveAction { get; set; }
    public string? Notes { get; set; }
}

public class VerifyNCRDto
{
    public Guid VerifiedById { get; set; }
    public DateTime VerifiedDate { get; set; }
}

public class NCRResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? WBSId { get; set; }
    public string NCRNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NCRCategory Category { get; set; }
    public NCRSeverity Severity { get; set; }
    public Guid RaisedById { get; set; }
    public DateTime RaisedDate { get; set; }
    public string? ResponsibleParty { get; set; }
    public string? CorrectiveAction { get; set; }
    public Guid? VerifiedById { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public NCRStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
