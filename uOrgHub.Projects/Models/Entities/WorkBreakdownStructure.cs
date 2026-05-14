using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_wbs")]
public class WorkBreakdownStructure : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? ParentWBSId { get; set; }
    public WorkBreakdownStructure? ParentWBS { get; set; }
    public ICollection<WorkBreakdownStructure> Children { get; set; } = new List<WorkBreakdownStructure>();

    [Required][MaxLength(50)]  public string WBSCode { get; set; } = string.Empty;
    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(1000)]          public string? Description { get; set; }

    public int Level { get; set; } = 1;
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public int PlannedDuration { get; set; }

    public WBSStatus Status { get; set; } = WBSStatus.NotStarted;

    [Column(TypeName = "decimal(5,2)")] public decimal CompletionPercent { get; set; }

    public int Sequence { get; set; }
}
