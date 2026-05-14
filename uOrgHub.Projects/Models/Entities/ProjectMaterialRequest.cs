using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_material_requests")]
public class ProjectMaterialRequest : BaseEntity
{
    [Required][MaxLength(20)] public string RequestNumber { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? WBSId { get; set; }
    public WorkBreakdownStructure? WBS { get; set; }

    public Guid RequestedById { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime RequiredDate { get; set; }

    public MaterialRequestStatus Status { get; set; } = MaterialRequestStatus.Draft;

    [MaxLength(1000)] public string? Notes { get; set; }

    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<ProjectMaterialRequestItem> Items { get; set; } = new List<ProjectMaterialRequestItem>();
}
