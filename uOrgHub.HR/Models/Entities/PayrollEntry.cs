using System.ComponentModel.DataAnnotations.Schema;
using uOrgHub.HR.Models.Enums;
using uOrgHub.Shared.Entities;

namespace uOrgHub.HR.Models.Entities;

[Table("hr_payroll_entries")]
public class PayrollEntry : BaseEntity
{
    public Guid PayrollCycleId { get; set; }
    public PayrollCycle PayrollCycle { get; set; } = null!;

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")] public decimal GrossSalary { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal BasicSalary { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalAllowances { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TotalDeductions { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal TaxAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal NetSalary { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal OvertimePay { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal BonusAmount { get; set; }

    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDays { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal OvertimeHours { get; set; }

    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public string? PayslipPath { get; set; }
}
