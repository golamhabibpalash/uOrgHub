using Microsoft.EntityFrameworkCore;
using uOrgHub.API.DTOs;
using uOrgHub.HR.Models.Entities;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Data;

namespace uOrgHub.API.Services;

public class HRDashboardService : IHRDashboardService
{
    private readonly AppDbContext _db;

    public HRDashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HRDashboardDto> GetAsync()
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var today = now.Date;

        var totalEmployees = await _db.Set<Employee>()
            .CountAsync(e => !e.IsDeleted && e.Status == EmployeeStatus.Active);

        var totalDepartments = await _db.Set<Department>()
            .CountAsync(d => !d.IsDeleted && d.IsActive);

        var openPositions = await _db.Set<JobPosting>()
            .CountAsync(j => !j.IsDeleted && j.Status == JobPostingStatus.Published);

        var pendingLeaveRequests = await _db.Set<LeaveRequest>()
            .CountAsync(l => !l.IsDeleted && l.Status == LeaveStatus.Pending);

        var activePayrollCycles = await _db.Set<PayrollCycle>()
            .CountAsync(p => !p.IsDeleted && (p.Status == PayrollStatus.Draft || p.Status == PayrollStatus.Processing));

        var activeTrainings = await _db.Set<TrainingProgram>()
            .CountAsync(t => !t.IsDeleted && (t.Status == TrainingStatus.Upcoming || t.Status == TrainingStatus.Ongoing));

        var newEmployeesThisMonth = await _db.Set<Employee>()
            .CountAsync(e => !e.IsDeleted && e.Status == EmployeeStatus.Active && e.JoiningDate >= firstOfMonth);

        var leaveRequestsThisMonth = await _db.Set<LeaveRequest>()
            .CountAsync(l => !l.IsDeleted && l.CreatedAt >= firstOfMonth);

        var activeJobPostings = await _db.Set<JobPosting>()
            .CountAsync(j => !j.IsDeleted && j.Status == JobPostingStatus.Published);

        var attendanceRate = await CalculateAttendanceRate(now);

        var employeesPerDepartment = await _db.Set<Employee>()
            .Where(e => !e.IsDeleted && e.Status == EmployeeStatus.Active)
            .GroupBy(e => e.Department.Name)
            .Select(g => new DepartmentEmployeeCountDto(g.Key, g.Count()))
            .ToListAsync();

        var recentActivities = await LoadRecentActivities(now);

        return new HRDashboardDto(
            totalEmployees,
            totalDepartments,
            openPositions,
            pendingLeaveRequests,
            activePayrollCycles,
            activeTrainings,
            newEmployeesThisMonth,
            leaveRequestsThisMonth,
            activeJobPostings,
            attendanceRate,
            employeesPerDepartment,
            recentActivities
        );
    }

    private async Task<decimal> CalculateAttendanceRate(DateTime now)
    {
        var firstOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var workingDays = await _db.Set<AttendanceLog>()
            .CountAsync(a => !a.IsDeleted && a.AttendanceDate >= firstOfMonth && a.AttendanceDate <= now);

        var totalActiveEmployees = await _db.Set<Employee>()
            .CountAsync(e => !e.IsDeleted && e.Status == EmployeeStatus.Active);

        if (totalActiveEmployees == 0) return 0;

        var businessDays = Math.Max(1, (now - firstOfMonth).Days);
        var expectedAttendance = totalActiveEmployees * businessDays;

        return expectedAttendance > 0
            ? Math.Round((decimal)workingDays / expectedAttendance * 100, 1)
            : 0;
    }

    private async Task<List<HRActivityDto>> LoadRecentActivities(DateTime now)
    {
        var activities = new List<HRActivityDto>();

        var recentEmployees = await _db.Set<Employee>()
            .Where(e => !e.IsDeleted && e.CreatedAt >= now.AddDays(-7))
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .ToListAsync();

        foreach (var emp in recentEmployees)
        {
            var timeAgo = FormatTimeAgo(now, emp.CreatedAt);
            activities.Add(new HRActivityDto(
                $"{emp.FirstName} {emp.LastName} added to system",
                timeAgo
            ));
        }

        var recentLeaves = await _db.Set<LeaveRequest>()
            .Where(l => !l.IsDeleted && l.CreatedAt >= now.AddDays(-7) && l.Status == LeaveStatus.Pending)
            .Include(l => l.Employee)
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .ToListAsync();

        foreach (var leave in recentLeaves)
        {
            var timeAgo = FormatTimeAgo(now, leave.CreatedAt);
            var name = leave.Employee?.FirstName ?? "";
            activities.Add(new HRActivityDto(
                $"Leave request submitted by {name}",
                timeAgo
            ));
        }

        var recentPayrolls = await _db.Set<PayrollCycle>()
            .Where(p => !p.IsDeleted && p.CreatedAt >= now.AddDays(-7))
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .ToListAsync();

        foreach (var payroll in recentPayrolls)
        {
            var timeAgo = FormatTimeAgo(now, payroll.CreatedAt);
            activities.Add(new HRActivityDto(
                $"Payroll cycle \"{payroll.Title}\" {(payroll.Status == PayrollStatus.Processed ? "processed" : "created")}",
                timeAgo
            ));
        }

        var recentInterviews = await _db.Set<InterviewSchedule>()
            .Where(i => !i.IsDeleted && i.CreatedAt >= now.AddDays(-7))
            .OrderByDescending(i => i.CreatedAt)
            .Take(3)
            .ToListAsync();

        foreach (var interview in recentInterviews)
        {
            var timeAgo = FormatTimeAgo(now, interview.CreatedAt);
            activities.Add(new HRActivityDto(
                $"Interview scheduled",
                timeAgo
            ));
        }

        return activities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
    }

    private static string FormatTimeAgo(DateTime now, DateTime dt)
    {
        var diff = now - dt;
        if (diff.TotalMinutes < 1) return "just now";
        if (diff.TotalHours < 1)
        {
            var m = (int)diff.TotalMinutes;
            return $"{m} min ago";
        }
        if (diff.TotalDays < 1)
        {
            var h = (int)diff.TotalHours;
            return $"{h} hour{(h > 1 ? "s" : "")} ago";
        }
        if (diff.TotalDays < 7)
        {
            var d = (int)diff.TotalDays;
            return $"{d} day{(d > 1 ? "s" : "")} ago";
        }
        return dt.ToString("MMM dd");
    }
}
