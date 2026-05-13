using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_work_schedules")]
public class WorkSchedule : BaseEntity
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalHours { get; set; }
    public bool IsFlexible { get; set; } = false;
    public int GracePeriodMinutes { get; set; } = 10;
    public int WorkingDaysPerWeek { get; set; } = 5;
    public bool IsActive { get; set; } = true;

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
