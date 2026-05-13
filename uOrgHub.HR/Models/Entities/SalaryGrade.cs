using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_salary_grades")]
public class SalaryGrade : BaseEntity
{
    [Required][MaxLength(20)]  public string GradeCode { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)]           public string? Description { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal MinSalary { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal MaxSalary { get; set; }
    [Column(TypeName = "decimal(5,2)")]  public decimal BasicPercentage { get; set; } = 60;
    public bool IsActive { get; set; } = true;

    public ICollection<Designation> Designations { get; set; } = new List<Designation>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
