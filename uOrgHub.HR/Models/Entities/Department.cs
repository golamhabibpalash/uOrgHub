using System.ComponentModel.DataAnnotations;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

public class Department : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Designation> Designations { get; set; } = new List<Designation>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
