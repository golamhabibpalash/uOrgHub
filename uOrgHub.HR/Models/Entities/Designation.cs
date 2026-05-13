using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_designations")]
public class Designation : BaseEntity
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required][MaxLength(20)]  public string Code { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public Guid? ParentDesignationId { get; set; }
    public Designation? ParentDesignation { get; set; }
    public ICollection<Designation> ChildDesignations { get; set; } = new List<Designation>();

    public Guid? SalaryGradeId { get; set; }
    public SalaryGrade? SalaryGrade { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
