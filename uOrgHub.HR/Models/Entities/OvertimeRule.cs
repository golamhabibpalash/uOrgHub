using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_overtime_rules")]
public class OvertimeRule : BaseEntity
{
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    public CalculationType CalculationType { get; set; } = CalculationType.PercentageOfBasic;
    [Column(TypeName = "decimal(5,2)")] public decimal Multiplier { get; set; } = 1.5m;
    public int MaxHoursPerMonth { get; set; } = 40;
    public bool AppliesWeekends { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
