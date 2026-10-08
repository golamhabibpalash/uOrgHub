using uOrgHub.HR.DTOs.Payroll;
using uOrgHub.HR.Models.Entities;

namespace uOrgHub.HR.Features.Payroll;

/// <summary>Payroll mappings shared by the command and query handlers.</summary>
internal static class PayrollMapping
{
    internal static PayrollCycleResponseDto CycleToDto(PayrollCycle e) => new()
    {
        Id = e.Id, Year = e.Year, Month = e.Month, Title = e.Title,
        StartDate = e.StartDate, EndDate = e.EndDate, ProcessedDate = e.ProcessedDate,
        Status = e.Status, TotalBasic = e.TotalBasic, TotalAllowances = e.TotalAllowances,
        TotalDeductions = e.TotalDeductions, TotalNetPay = e.TotalNetPay,
        TotalEmployees = e.TotalEmployees, Remarks = e.Remarks, CreatedAt = e.CreatedAt
    };

    internal static List<StructureComponent> ToStructureComponents(EmployeeSalaryStructure s) =>
        s.Components
            .Where(c => !c.IsDeleted && c.IsActive && c.SalaryComponent != null)
            .Select(c => new StructureComponent(
                c.SalaryComponentId, c.SalaryComponent.Code, c.SalaryComponent.Name, c.SalaryComponent.ComponentType,
                c.SalaryComponent.CalculationType, c.Value, c.SalaryComponent.SortOrder))
            .ToList();

    /// <summary>Needs Employee (+Designation), SalaryGrade and Components.SalaryComponent loaded.</summary>
    internal static EmployeeSalaryStructureResponseDto StructureToDto(EmployeeSalaryStructure s)
    {
        var components = ToStructureComponents(s);
        var monthly = PayrollCalculator.Monthly(s.BasicSalary, components);
        var amounts = monthly.Lines.Where(l => l.ComponentId.HasValue).ToDictionary(l => l.ComponentId!.Value, l => l.Amount);

        return new EmployeeSalaryStructureResponseDto
        {
            Id = s.Id, EmployeeId = s.EmployeeId,
            EmployeeName = s.Employee != null ? $"{s.Employee.FirstName} {s.Employee.LastName}" : string.Empty,
            EmployeeCode = s.Employee?.EmployeeCode ?? string.Empty,
            DesignationName = s.Employee?.Designation?.Name,
            SalaryGradeId = s.SalaryGradeId,
            SalaryGradeName = s.SalaryGrade != null ? $"{s.SalaryGrade.GradeCode} — {s.SalaryGrade.Name}" : string.Empty,
            BasicSalary = monthly.Basic, TotalAllowances = monthly.Allowances, GrossSalary = monthly.Gross,
            TotalDeductions = monthly.Deductions + monthly.Tax, NetSalary = monthly.Net,
            EffectiveDate = s.EffectiveDate, EndDate = s.EndDate, IsActive = s.IsActive,
            Components = components.OrderBy(c => PayrollCalculator.IsDeduction(c.Type)).ThenBy(c => c.SortOrder)
                .Select(c => new SalaryStructureComponentDto
                {
                    SalaryComponentId = c.ComponentId, Code = c.Code, Name = c.Name,
                    ComponentType = c.Type, CalculationType = c.CalculationType, Value = c.Value,
                    Amount = amounts.GetValueOrDefault(c.ComponentId),
                    IsDeduction = PayrollCalculator.IsDeduction(c.Type)
                }).ToList(),
            CreatedAt = s.CreatedAt
        };
    }
}
