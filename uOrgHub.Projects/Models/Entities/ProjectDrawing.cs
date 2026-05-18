using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_drawings")]
public class ProjectDrawing : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    [Required][MaxLength(50)]   public string DrawingNumber { get; set; } = string.Empty;
    [Required][MaxLength(300)]  public string Title { get; set; } = string.Empty;
    [MaxLength(10)]             public string Revision { get; set; } = "A";

    public DrawingDiscipline Discipline { get; set; }
    public DrawingStatus Status { get; set; } = DrawingStatus.Draft;

    public Guid? DrawnById { get; set; }
    public Guid? CheckedById { get; set; }
    public Guid? ApprovedById { get; set; }

    public DateTime? IssuedDate { get; set; }

    [MaxLength(1000)] public string? FilePath { get; set; }
    [MaxLength(500)]  public string? Notes { get; set; }
}
