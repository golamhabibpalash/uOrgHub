using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_onboarding_checklists")]
public class OnboardingChecklist : BaseEntity
{
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)]          public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public Guid? DesignationId { get; set; }
    public Designation? Designation { get; set; }

    public ICollection<OnboardingTask> Tasks { get; set; } = new List<OnboardingTask>();
}
