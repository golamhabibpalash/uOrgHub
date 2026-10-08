using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

/// <summary>
/// One payslip line — basic, an allowance, overtime, a deduction, tax — as calculated when the cycle
/// was processed. Name/code are copied from the salary component so a later rename doesn't
/// rewrite past payslips.
/// </summary>
[Table("hr_payroll_entry_lines")]
public class PayrollEntryLine : BaseEntity
{
    public Guid PayrollEntryId { get; set; }
    public PayrollEntry PayrollEntry { get; set; } = null!;

    public Guid? SalaryComponentId { get; set; }

    [Required][MaxLength(30)]  public string Code { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string Name { get; set; } = string.Empty;
    public PayslipLineType LineType { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    public int SortOrder { get; set; }
}
