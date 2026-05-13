using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_goals")]
public class Goal : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid ReviewCycleId { get; set; }
    public ReviewCycle ReviewCycle { get; set; } = null!;

    public Guid? KPIId { get; set; }
    public KPI? KPI { get; set; }

    [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(2000)]          public string? Description { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal? TargetValue { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal? AchievedValue { get; set; }
    [Column(TypeName = "decimal(5,2)")]  public decimal Weight { get; set; } = 100;
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
    public DateTime? DueDate { get; set; }
    [MaxLength(500)] public string? Remarks { get; set; }
}
