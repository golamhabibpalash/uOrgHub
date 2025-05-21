namespace OrgHub.Domain.Entities.HRM;

public class Attendance : CommonProps
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan CheckOutTime { get; set; }
    public string Status { get; set; } = default!;
    public string? Remarks { get; set; } = default!;
}
