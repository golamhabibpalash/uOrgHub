using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_employee_onboardings")]
public class EmployeeOnboarding : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid OnboardingChecklistId { get; set; }
    public OnboardingChecklist OnboardingChecklist { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;

    public ICollection<OnboardingTaskProgress> TaskProgresses { get; set; } = new List<OnboardingTaskProgress>();
}
