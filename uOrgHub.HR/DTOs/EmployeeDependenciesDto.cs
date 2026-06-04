namespace uOrgHub.HR.DTOs;

public class EmployeeDependenciesDto
{
    public Guid EmployeeId { get; set; }
    public bool HasUserAccount { get; set; }
    public int LeaveRequests { get; set; }
    public int AttendanceLogs { get; set; }
    public int PayrollEntries { get; set; }
    public int AssetAssignments { get; set; }
    public int TrainingEnrollments { get; set; }
    public int ExpenseRequests { get; set; }
    public int DirectReports { get; set; }
    public bool CanDelete { get; set; }
    public string? BlockingReason { get; set; }
}
