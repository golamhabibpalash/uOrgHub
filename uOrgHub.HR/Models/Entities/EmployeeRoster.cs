using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_employee_rosters")]
public class EmployeeRoster : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid ShiftId { get; set; }
    public Shift Shift { get; set; } = null!;

    public DateTime RosterDate { get; set; }
    public bool IsOff { get; set; } = false;
    [MaxLength(500)] public string? Note { get; set; }
}
