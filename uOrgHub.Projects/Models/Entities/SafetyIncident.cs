using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_safety_incidents")]
public class SafetyIncident : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Required][MaxLength(20)]  public string IncidentNumber { get; set; } = string.Empty;
    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)]          public string? Description { get; set; }

    public DateTime IncidentDate { get; set; }
    [MaxLength(300)] public string? Location { get; set; }

    public Guid ReportedById { get; set; }

    public SafetyIncidentSeverity Severity { get; set; }
    public SafetyIncidentStatus Status { get; set; } = SafetyIncidentStatus.Reported;

    [MaxLength(300)] public string? InjuredPersonName { get; set; }
    [MaxLength(200)] public string? InjuryType { get; set; }

    [MaxLength(2000)] public string? RootCause { get; set; }
    [MaxLength(2000)] public string? CorrectiveAction { get; set; }
    [MaxLength(2000)] public string? PreventiveAction { get; set; }

    public Guid? InvestigatedById { get; set; }
    public DateTime? InvestigationDate { get; set; }
    public DateTime? ClosedDate { get; set; }
}
