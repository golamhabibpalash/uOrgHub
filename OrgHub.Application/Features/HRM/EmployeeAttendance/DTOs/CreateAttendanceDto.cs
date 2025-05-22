namespace OrgHub.Application.Features.HRM.EmployeeAttendance;
public class CreateAttendanceDto
{
    public required int EmployeeId { get; set; }
    public required DateTime AttendanceDate { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan CheckOutTime { get; set; }
    public required string AttendanceType { get; set; }
}