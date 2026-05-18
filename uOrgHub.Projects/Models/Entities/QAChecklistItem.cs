using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_qa_checklist_items")]
public class QAChecklistItem : BaseEntity
{
    public Guid ChecklistId { get; set; }
    public QAChecklist Checklist { get; set; } = null!;

    [Required][MaxLength(500)] public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public int Sequence { get; set; }

    public InspectionResult? Result { get; set; }
    [MaxLength(500)] public string? Remarks { get; set; }
}
