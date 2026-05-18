using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_ncrs")]
public class NonConformanceReport : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    [Required][MaxLength(20)]  public string NCRNumber { get; set; } = string.Empty;
    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)]          public string? Description { get; set; }

    public NCRCategory Category { get; set; }
    public NCRSeverity Severity { get; set; }

    public Guid RaisedById { get; set; }
    public DateTime RaisedDate { get; set; }

    [MaxLength(300)] public string? ResponsibleParty { get; set; }
    [MaxLength(2000)] public string? CorrectiveAction { get; set; }

    public Guid? VerifiedById { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public DateTime? ClosedDate { get; set; }

    public NCRStatus Status { get; set; } = NCRStatus.Open;

    [MaxLength(500)] public string? Notes { get; set; }
}
