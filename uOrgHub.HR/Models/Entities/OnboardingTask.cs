using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_onboarding_tasks")]
public class OnboardingTask : BaseEntity
{
    public Guid OnboardingChecklistId { get; set; }
    public OnboardingChecklist OnboardingChecklist { get; set; } = null!;

    [Required][MaxLength(200)] public string TaskName { get; set; } = string.Empty;
    [MaxLength(1000)]          public string? Description { get; set; }
    [MaxLength(100)]           public string AssignedTeam { get; set; } = "HR";
    public int DueDaysFromJoining { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
    public bool IsRequired { get; set; } = true;
}
