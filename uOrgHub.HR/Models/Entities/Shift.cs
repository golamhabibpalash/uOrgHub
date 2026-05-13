using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_shifts")]
public class Shift : BaseEntity
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(20)]            public string? Code { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsNightShift { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public Guid WorkScheduleId { get; set; }
    public WorkSchedule WorkSchedule { get; set; } = null!;

    public ICollection<EmployeeRoster> Rosters { get; set; } = new List<EmployeeRoster>();
}
