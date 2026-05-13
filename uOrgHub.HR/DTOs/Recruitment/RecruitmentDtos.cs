using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs.Recruitment;

public class CreateJobPostingDto
{
    public string JobCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public int RequiredCount { get; set; } = 1;
    public int ExperienceYearsMin { get; set; } = 0;
    public int ExperienceYearsMax { get; set; } = 0;
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid DesignationId { get; set; }
}

public class UpdateJobPostingDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public int RequiredCount { get; set; }
    public JobPostingStatus Status { get; set; }
    public DateTime? ClosingDate { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
}

public class JobPostingResponseDto
{
    public Guid Id { get; set; }
    public string JobCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public int RequiredCount { get; set; }
    public int ExperienceYearsMin { get; set; }
    public int ExperienceYearsMax { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public JobPostingStatus Status { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid DesignationId { get; set; }
    public string DesignationName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateCandidateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? PortfolioUrl { get; set; }
    public int TotalExperienceYears { get; set; }
    public decimal? CurrentSalary { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? CurrentCompany { get; set; }
    public string? CurrentDesignation { get; set; }
    public string? Source { get; set; }
    public string? ReferredBy { get; set; }
    public Gender? Gender { get; set; }
    public string? Skills { get; set; }
}

public class CandidateResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int TotalExperienceYears { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string? CurrentCompany { get; set; }
    public string? Skills { get; set; }
    public Gender? Gender { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateJobApplicationDto
{
    public Guid CandidateId { get; set; }
    public Guid JobPostingId { get; set; }
    public string? CoverLetter { get; set; }
    public string? Notes { get; set; }
}

public class UpdateJobApplicationDto
{
    public ApplicationStatus Status { get; set; }
    public string? Notes { get; set; }
    public int? HiringScore { get; set; }
}

public class JobApplicationResponseDto
{
    public Guid Id { get; set; }
    public Guid CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public Guid JobPostingId { get; set; }
    public string JobPostingTitle { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public ApplicationStatus Status { get; set; }
    public string? Notes { get; set; }
    public int? HiringScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInterviewScheduleDto
{
    public Guid JobApplicationId { get; set; }
    public InterviewType InterviewType { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    public string? InterviewerIds { get; set; }
}

public class UpdateInterviewScheduleDto
{
    public DateTime ScheduledAt { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
    public string? CancellationReason { get; set; }
}

public class InterviewScheduleResponseDto
{
    public Guid Id { get; set; }
    public Guid JobApplicationId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public InterviewType InterviewType { get; set; }
    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; }
    public string? Location { get; set; }
    public string? MeetingLink { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? Feedback { get; set; }
    public int? Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOnboardingChecklistDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public Guid? DesignationId { get; set; }
}

public class OnboardingChecklistResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public Guid? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeOnboardingDto
{
    public Guid EmployeeId { get; set; }
    public Guid OnboardingChecklistId { get; set; }
    public DateTime StartDate { get; set; }
}

public class UpdateEmployeeOnboardingDto
{
    public OnboardingStatus Status { get; set; }
    public DateTime? CompletionDate { get; set; }
}

public class EmployeeOnboardingResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid OnboardingChecklistId { get; set; }
    public string ChecklistName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public OnboardingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
