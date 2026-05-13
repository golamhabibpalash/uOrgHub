using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_onboarding_task_progress")]
public class OnboardingTaskProgress : BaseEntity
{
    public Guid EmployeeOnboardingId { get; set; }
    public EmployeeOnboarding EmployeeOnboarding { get; set; } = null!;

    public Guid OnboardingTaskId { get; set; }
    public OnboardingTask OnboardingTask { get; set; } = null!;

    public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
    public DateTime? CompletedDate { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
    public Guid? CompletedBy { get; set; }
}
