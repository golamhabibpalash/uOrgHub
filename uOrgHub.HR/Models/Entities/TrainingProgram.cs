using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_training_programs")]
public class TrainingProgram : BaseEntity
{
    [Required][MaxLength(200)] public string Title { get; set; } = string.Empty;
    [MaxLength(1000)]          public string? Description { get; set; }
    [MaxLength(100)]           public string? Category { get; set; }
    public TrainingMode Mode { get; set; } = TrainingMode.InPerson;
    public int DurationHours { get; set; }
    [MaxLength(200)] public string? Provider { get; set; }
    [MaxLength(500)] public string? Location { get; set; }
    public int MaxParticipants { get; set; } = 30;
    [Column(TypeName = "decimal(18,2)")] public decimal? Cost { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TrainingStatus Status { get; set; } = TrainingStatus.Upcoming;
    public bool HasCertificate { get; set; } = false;

    public ICollection<EmployeeTraining> EmployeeTrainings { get; set; } = new List<EmployeeTraining>();
}
