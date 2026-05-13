using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_employee_trainings")]
public class EmployeeTraining : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid TrainingProgramId { get; set; }
    public TrainingProgram TrainingProgram { get; set; } = null!;

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletionDate { get; set; }
    public OnboardingStatus Status { get; set; } = OnboardingStatus.Pending;
    [Column(TypeName = "decimal(5,2)")] public decimal? Score { get; set; }
    [MaxLength(500)] public string? CertificatePath { get; set; }
    [MaxLength(500)] public string? Remarks { get; set; }
}
