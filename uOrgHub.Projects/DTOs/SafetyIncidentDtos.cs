using uOrgHub.Projects.Models.Enums;

namespace uOrgHub.Projects.DTOs;

public class CreateSafetyIncidentDto
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime IncidentDate { get; set; }
    public string? Location { get; set; }
    public Guid ReportedById { get; set; }
    public SafetyIncidentSeverity Severity { get; set; }
    public string? InjuredPersonName { get; set; }
    public string? InjuryType { get; set; }
}

public class UpdateSafetyIncidentDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime IncidentDate { get; set; }
    public string? Location { get; set; }
    public SafetyIncidentSeverity Severity { get; set; }
    public SafetyIncidentStatus Status { get; set; }
    public string? InjuredPersonName { get; set; }
    public string? InjuryType { get; set; }
    public string? RootCause { get; set; }
    public string? CorrectiveAction { get; set; }
    public string? PreventiveAction { get; set; }
    public Guid? InvestigatedById { get; set; }
    public DateTime? InvestigationDate { get; set; }
    public DateTime? ClosedDate { get; set; }
}

public class SafetyIncidentResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string IncidentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime IncidentDate { get; set; }
    public string? Location { get; set; }
    public Guid ReportedById { get; set; }
    public SafetyIncidentSeverity Severity { get; set; }
    public SafetyIncidentStatus Status { get; set; }
    public string? InjuredPersonName { get; set; }
    public string? InjuryType { get; set; }
    public string? RootCause { get; set; }
    public string? CorrectiveAction { get; set; }
    public string? PreventiveAction { get; set; }
    public Guid? InvestigatedById { get; set; }
    public DateTime? InvestigationDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
