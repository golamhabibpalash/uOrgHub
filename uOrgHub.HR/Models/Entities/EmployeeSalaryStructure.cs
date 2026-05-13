using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_employee_salary_structures")]
public class EmployeeSalaryStructure : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid SalaryGradeId { get; set; }
    public SalaryGrade SalaryGrade { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")] public decimal GrossSalary { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal BasicSalary { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeSalaryComponent> Components { get; set; } = new List<EmployeeSalaryComponent>();
}
