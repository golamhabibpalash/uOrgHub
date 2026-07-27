using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Projects.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_project_status_logs")]
public class ProjectStatusLog : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ProjectStatus FromStatus { get; set; }
    public ProjectStatus ToStatus { get; set; }

    [Column(TypeName = "varchar(500)")]
    public string? Reason { get; set; }
}
