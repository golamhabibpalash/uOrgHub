using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_submittals")]
public class ProjectSubmittal : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Required][MaxLength(20)]  public string SubmittalNumber { get; set; } = string.Empty;
    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(100)]           public string? ContractorReference { get; set; }
    [MaxLength(1000)]          public string? Description { get; set; }

    public SubmittalStatus Status { get; set; } = SubmittalStatus.Draft;

    public Guid SubmittedById { get; set; }
    public DateTime? SubmittedDate { get; set; }

    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewDate { get; set; }

    [MaxLength(2000)] public string? ReviewComments { get; set; }
    [MaxLength(1000)] public string? FilePath { get; set; }
    [MaxLength(500)]  public string? Notes { get; set; }
}
