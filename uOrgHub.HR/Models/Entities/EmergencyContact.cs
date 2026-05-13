using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_emergency_contacts")]
public class EmergencyContact : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required][MaxLength(50)]  public string Relationship { get; set; } = string.Empty;
    [Required][MaxLength(20)]  public string Phone { get; set; } = string.Empty;
    [MaxLength(20)]            public string? AlternatePhone { get; set; }
    [MaxLength(500)]           public string? Address { get; set; }
    public bool IsPrimary { get; set; } = false;
}
