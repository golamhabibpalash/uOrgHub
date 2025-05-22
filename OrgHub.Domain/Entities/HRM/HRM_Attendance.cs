namespace OrgHub.Domain.Entities.HRM;

public class HRM_Attendance : CommonProps
{
    public long Id { get; set; }
    public required int EmployeeId { get; set; }
    public required DateTime AttendanceDate { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public required string AttendanceType { get; set; }
    public string? Remarks { get; set; } = default!;

    public required virtual HRM_Employee Employee { get; set; }
}
