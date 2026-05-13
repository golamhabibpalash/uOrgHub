using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_departments")]
public class Department : BaseEntity
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required][MaxLength(20)]  public string Code { get; set; } = string.Empty;
    [MaxLength(500)]           public string? Description { get; set; }
    public DepartmentType Type { get; set; } = DepartmentType.Other;
    public bool IsActive { get; set; } = true;

    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

    public Guid? HeadOfDepartmentId { get; set; }
    public Employee? HeadOfDepartment { get; set; }

    public ICollection<Designation> Designations { get; set; } = new List<Designation>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
