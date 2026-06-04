namespace uOrgHub.API.DTOs;

public record HRDashboardDto(
    int TotalEmployees,
    int TotalDepartments,
    int OpenPositions,
    int PendingLeaveRequests,
    int ActivePayrollCycles,
    int ActiveTrainings,
    int NewEmployeesThisMonth,
    int LeaveRequestsThisMonth,
    int ActiveJobPostings,
    decimal AttendanceRate,
    List<DepartmentEmployeeCountDto> EmployeesPerDepartment,
    List<HRActivityDto> RecentActivities
);

public record DepartmentEmployeeCountDto(
    string DepartmentName,
    int EmployeeCount
);

public record HRActivityDto(
    string Description,
    string Timestamp
);
