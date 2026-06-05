using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.DTOs.Payroll;

public class CreateSalaryGradeDto
{
    public string GradeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSalaryGradeDto
{
    public string GradeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SalaryGradeResponseDto
{
    public Guid Id { get; set; }
    public string GradeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSalaryComponentDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public SalaryComponentType ComponentType { get; set; }
    public CalculationType CalculationType { get; set; } = CalculationType.Fixed;
    public decimal DefaultValue { get; set; }
    public bool IsTaxable { get; set; } = false;
    public bool IsFixed { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public string? Description { get; set; }
}

public class UpdateSalaryComponentDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public SalaryComponentType ComponentType { get; set; }
    public CalculationType CalculationType { get; set; } = CalculationType.Fixed;
    public decimal DefaultValue { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsFixed { get; set; } = true;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
}

public class SalaryComponentResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public SalaryComponentType ComponentType { get; set; }
    public CalculationType CalculationType { get; set; }
    public decimal DefaultValue { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsFixed { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeSalaryStructureDto
{
    public Guid EmployeeId { get; set; }
    public Guid SalaryGradeId { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal BasicSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public class UpdateEmployeeSalaryStructureDto
{
    public decimal GrossSalary { get; set; }
    public decimal BasicSalary { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
}

public class EmployeeSalaryStructureResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid SalaryGradeId { get; set; }
    public string SalaryGradeName { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal BasicSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePayrollCycleDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class UpdatePayrollCycleDto
{
    public PayrollStatus Status { get; set; }
    public string? Remarks { get; set; }
}

public class PayrollCycleResponseDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public PayrollStatus Status { get; set; }
    public decimal TotalBasic { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }
    public int TotalEmployees { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PayrollEntryResponseDto
{
    public Guid Id { get; set; }
    public Guid PayrollCycleId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetSalary { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal BonusAmount { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public PayrollStatus Status { get; set; }
    public string? PayslipPath { get; set; }
}

public class CreateOvertimeRuleDto
{
    public string Name { get; set; } = string.Empty;
    public CalculationType CalculationType { get; set; } = CalculationType.PercentageOfBasic;
    public decimal Multiplier { get; set; } = 1.5m;
    public int MaxHoursPerMonth { get; set; } = 40;
    public bool AppliesWeekends { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class OvertimeRuleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CalculationType CalculationType { get; set; }
    public decimal Multiplier { get; set; }
    public int MaxHoursPerMonth { get; set; }
    public bool AppliesWeekends { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateExpenseRequestDto
{
    public Guid EmployeeId { get; set; }
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReceiptFilePath { get; set; }
}

public class UpdateExpenseRequestDto
{
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReceiptFilePath { get; set; }
}

public class ApproveExpenseDto
{
    public Guid ApproverId { get; set; }
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}

public class ExpenseRequestResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReceiptFilePath { get; set; }
    public ExpenseStatus Status { get; set; }
    public Guid? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
