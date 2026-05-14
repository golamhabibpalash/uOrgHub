using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.Projects.Models.Entities;

[Table("proj_project_categories")]
public class ProjectCategory : BaseEntity
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required][MaxLength(20)]  public string Code { get; set; } = string.Empty;
    [MaxLength(500)]           public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
