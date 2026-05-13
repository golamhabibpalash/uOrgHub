using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_kpis")]
public class KPI : BaseEntity
{
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)]          public string? Description { get; set; }
    [MaxLength(100)]           public string? MeasurementUnit { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal? TargetValue { get; set; }
    [Column(TypeName = "decimal(5,2)")]  public decimal Weight { get; set; } = 100;
    public bool IsActive { get; set; } = true;

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? DesignationId { get; set; }
    public Designation? Designation { get; set; }

    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
