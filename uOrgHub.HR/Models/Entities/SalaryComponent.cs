using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_salary_components")]
public class SalaryComponent : BaseEntity
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required][MaxLength(30)]  public string Code { get; set; } = string.Empty;
    public SalaryComponentType ComponentType { get; set; }
    public CalculationType CalculationType { get; set; } = CalculationType.Fixed;
    [Column(TypeName = "decimal(18,2)")] public decimal DefaultValue { get; set; }
    public bool IsTaxable { get; set; } = false;
    public bool IsFixed { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    [MaxLength(500)] public string? Description { get; set; }
}
