using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_employee_salary_components")]
public class EmployeeSalaryComponent : BaseEntity
{
    public Guid EmployeeSalaryStructureId { get; set; }
    public EmployeeSalaryStructure SalaryStructure { get; set; } = null!;

    public Guid SalaryComponentId { get; set; }
    public SalaryComponent SalaryComponent { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")] public decimal Value { get; set; }
    public bool IsActive { get; set; } = true;
}
