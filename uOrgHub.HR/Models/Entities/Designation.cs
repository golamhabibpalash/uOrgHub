using System.ComponentModel.DataAnnotations;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

public class Designation : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
