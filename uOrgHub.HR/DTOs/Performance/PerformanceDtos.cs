using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs.Performance;

public class CreateReviewCycleDto
{
    public string Name { get; set; } = string.Empty;
    public ReviewCycleType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
}

public class UpdateReviewCycleDto
{
    public string Name { get; set; } = string.Empty;
    public ReviewStatus Status { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
}

public class ReviewCycleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ReviewCycleType Type { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReviewStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateKPIDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MeasurementUnit { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal Weight { get; set; } = 100;
    public bool IsActive { get; set; } = true;
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
}

public class KPIResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MeasurementUnit { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal Weight { get; set; }
    public bool IsActive { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateGoalDto
{
    public Guid EmployeeId { get; set; }
    public Guid ReviewCycleId { get; set; }
    public Guid? KPIId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal Weight { get; set; } = 100;
    public DateTime? DueDate { get; set; }
}

public class UpdateGoalDto
{
    public decimal? AchievedValue { get; set; }
    public GoalStatus Status { get; set; }
    public string? Remarks { get; set; }
}

public class GoalResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid ReviewCycleId { get; set; }
    public string ReviewCycleName { get; set; } = string.Empty;
    public Guid? KPIId { get; set; }
    public string? KPIName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? AchievedValue { get; set; }
    public decimal Weight { get; set; }
    public GoalStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePerformanceReviewDto
{
    public Guid ReviewCycleId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public PerformanceReviewType ReviewType { get; set; }
    public DateTime DueDate { get; set; }
}

public class UpdatePerformanceReviewDto
{
    public decimal? OverallRating { get; set; }
    public string? Comments { get; set; }
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public ReviewStatus Status { get; set; }
}

public class PerformanceReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid ReviewCycleId { get; set; }
    public string ReviewCycleName { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public PerformanceReviewType ReviewType { get; set; }
    public decimal? OverallRating { get; set; }
    public string? Comments { get; set; }
    public string? Strengths { get; set; }
    public string? AreasForImprovement { get; set; }
    public ReviewStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTrainingProgramDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public TrainingMode Mode { get; set; } = TrainingMode.InPerson;
    public int DurationHours { get; set; }
    public string? Provider { get; set; }
    public string? Location { get; set; }
    public int MaxParticipants { get; set; } = 30;
    public decimal? Cost { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool HasCertificate { get; set; } = false;
}

public class UpdateTrainingProgramDto
{
    public string? Description { get; set; }
    public string? Location { get; set; }
    public TrainingStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class TrainingProgramResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public TrainingMode Mode { get; set; }
    public int DurationHours { get; set; }
    public string? Provider { get; set; }
    public string? Location { get; set; }
    public int MaxParticipants { get; set; }
    public decimal? Cost { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TrainingStatus Status { get; set; }
    public bool HasCertificate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeTrainingDto
{
    public Guid EmployeeId { get; set; }
    public Guid TrainingProgramId { get; set; }
    public DateTime EnrollmentDate { get; set; }
}

public class UpdateEmployeeTrainingDto
{
    public OnboardingStatus Status { get; set; }
    public DateTime? CompletionDate { get; set; }
    public decimal? Score { get; set; }
    public string? CertificatePath { get; set; }
    public string? Remarks { get; set; }
}

public class EmployeeTrainingResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid TrainingProgramId { get; set; }
    public string TrainingTitle { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public OnboardingStatus Status { get; set; }
    public decimal? Score { get; set; }
    public string? CertificatePath { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}
