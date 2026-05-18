using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_rfis")]
public class ProjectRFI : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    [Required][MaxLength(20)]   public string RFINumber { get; set; } = string.Empty;
    [Required][MaxLength(300)]  public string Subject { get; set; } = string.Empty;
    [MaxLength(2000)]           public string? Description { get; set; }

    public Guid RaisedById { get; set; }
    public DateTime RaisedDate { get; set; }

    public Guid? AssignedToId { get; set; }
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResponseDate { get; set; }

    [MaxLength(2000)] public string? Response { get; set; }

    public RFIStatus Status { get; set; } = RFIStatus.Open;
    public bool IsUrgent { get; set; } = false;

    [MaxLength(500)] public string? Notes { get; set; }
}
