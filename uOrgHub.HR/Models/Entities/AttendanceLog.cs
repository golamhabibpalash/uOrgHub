using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_attendance_logs")]
public class AttendanceLog : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DateTime AttendanceDate { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public AttendanceSource Source { get; set; } = AttendanceSource.Manual;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

    [Column(TypeName = "decimal(5,2)")] public decimal WorkHours { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal OvertimeHours { get; set; }

    [MaxLength(200)] public string? Location { get; set; }
    [MaxLength(500)] public string? Remarks { get; set; }
    public bool IsManuallyEdited { get; set; } = false;
    public Guid? EditedBy { get; set; }
}
