using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_qa_checklists")]
public class QAChecklist : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    [Required][MaxLength(20)]  public string ChecklistNumber { get; set; } = string.Empty;
    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;

    public InspectionType InspectionType { get; set; }
    public QAChecklistStatus Status { get; set; } = QAChecklistStatus.Draft;

    public Guid? InspectedById { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? InspectedDate { get; set; }

    public InspectionResult? OverallResult { get; set; }

    [MaxLength(1000)] public string? Remarks { get; set; }

    public ICollection<QAChecklistItem> Items { get; set; } = new List<QAChecklistItem>();
}
