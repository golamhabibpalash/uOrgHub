using uOrgHub.HR.Models.Enums;

namespace uOrgHub.HR.Features.Payroll;

/// <summary>A salary component as assigned in an employee's salary structure.</summary>
public record StructureComponent(
    Guid ComponentId, string Code, string Name, SalaryComponentType Type,
    CalculationType CalculationType, decimal Value, int SortOrder);

public record PayLine(Guid? ComponentId, string Code, string Name, PayslipLineType LineType, decimal Amount, int SortOrder);

public record OvertimePolicy(CalculationType CalculationType, decimal Multiplier, int MaxHoursPerMonth);

public record SalaryBreakdown(decimal Basic, decimal Allowances, decimal Gross, decimal Deductions, decimal Tax, decimal Net, IReadOnlyList<PayLine> Lines);

public record PayrollInput(
    DateTime PeriodStart, DateTime PeriodEnd, DateTime JoiningDate,
    decimal Basic, IReadOnlyList<StructureComponent> Components,
    int PresentDays, int AbsentDays, decimal PaidLeaveDays, decimal UnpaidLeaveDays,
    decimal OvertimeHours, OvertimePolicy? Overtime);

public record PayrollResult(
    int TotalDays, decimal UnpaidDays, decimal Basic, decimal Allowances, decimal OvertimePay,
    decimal Gross, decimal Deductions, decimal Tax, decimal Net, IReadOnlyList<PayLine> Lines);

/// <summary>
/// The salary rules, kept free of EF so they can be tested directly.
///
/// Monthly salary = basic + allowances (gross). Deductions (PF, loan, advance, other) and tax come
/// off gross. When a cycle is processed, unpaid days — explicit Absent attendance, approved unpaid
/// leave, and days before the joining date — are deducted at gross ÷ days in the period, and
/// overtime is paid on top.
/// </summary>
public static class PayrollCalculator
{
    /// <summary>Hourly rate base for overtime: 26 working days × 8 hours.</summary>
    public const decimal StandardMonthlyHours = 208m;

    public static bool IsDeduction(SalaryComponentType type) =>
        type is SalaryComponentType.PF or SalaryComponentType.Tax or SalaryComponentType.Loan
             or SalaryComponentType.Advance or SalaryComponentType.OtherDeduction;

    /// <summary>The monthly salary a structure describes, before attendance.</summary>
    public static SalaryBreakdown Monthly(decimal basic, IReadOnlyList<StructureComponent> components)
    {
        var lines = new List<PayLine> { new(null, "BASIC", "Basic Salary", PayslipLineType.Earning, Round(basic), 0) };

        foreach (var c in components.Where(c => !IsDeduction(c.Type) && c.Type != SalaryComponentType.BasicSalary).OrderBy(c => c.SortOrder))
        {
            // An allowance can't be a % of gross: gross is itself the sum of the allowances.
            var amount = c.CalculationType == CalculationType.PercentageOfBasic ? basic * c.Value / 100m : c.Value;
            lines.Add(new(c.ComponentId, c.Code, c.Name, PayslipLineType.Earning, Round(amount), c.SortOrder + 1));
        }

        var gross = lines.Sum(l => l.Amount);

        foreach (var c in components.Where(c => IsDeduction(c.Type)).OrderBy(c => c.SortOrder))
        {
            var amount = c.CalculationType switch
            {
                CalculationType.PercentageOfBasic => basic * c.Value / 100m,
                CalculationType.PercentageOfGross => gross * c.Value / 100m,
                _ => c.Value
            };
            lines.Add(new(c.ComponentId, c.Code, c.Name, PayslipLineType.Deduction, Round(amount), 500 + c.SortOrder));
        }

        return Summarize(basic, lines, components);
    }

    public static PayrollResult Calculate(PayrollInput input)
    {
        var start = input.PeriodStart.Date;
        var end = input.PeriodEnd.Date;
        var totalDays = (end - start).Days + 1;

        var monthly = Monthly(input.Basic, input.Components);
        var lines = monthly.Lines.ToList();
        var monthlyGross = monthly.Gross;

        var notEmployedDays = input.JoiningDate.Date > start
            ? Math.Min(totalDays, (input.JoiningDate.Date - start).Days)
            : 0;
        var unpaidDays = Math.Min(totalDays, input.AbsentDays + input.UnpaidLeaveDays + notEmployedDays);
        if (unpaidDays > 0)
        {
            var deduction = Round(monthlyGross / totalDays * unpaidDays);
            lines.Add(new(null, "UNPAID", $"Unpaid days ({unpaidDays:0.#})", PayslipLineType.Deduction, deduction, 950));
        }

        decimal overtimePay = 0;
        if (input.Overtime is { } rule && input.OvertimeHours > 0)
        {
            var hours = rule.MaxHoursPerMonth > 0 ? Math.Min(input.OvertimeHours, rule.MaxHoursPerMonth) : input.OvertimeHours;
            var hourly = rule.CalculationType switch
            {
                CalculationType.Fixed => rule.Multiplier, // a fixed amount per hour
                CalculationType.PercentageOfGross => monthlyGross / StandardMonthlyHours * rule.Multiplier,
                _ => input.Basic / StandardMonthlyHours * rule.Multiplier
            };
            overtimePay = Round(hours * hourly);
            if (overtimePay > 0)
                lines.Add(new(null, "OT", $"Overtime ({hours:0.##} h)", PayslipLineType.Earning, overtimePay, 900));
        }

        var summary = Summarize(input.Basic, lines, input.Components);
        return new PayrollResult(
            totalDays, unpaidDays, summary.Basic, summary.Allowances - overtimePay, overtimePay,
            summary.Gross, summary.Deductions, summary.Tax, summary.Net,
            lines.OrderBy(l => l.LineType).ThenBy(l => l.SortOrder).ToList());
    }

    private static SalaryBreakdown Summarize(decimal basic, List<PayLine> lines, IReadOnlyList<StructureComponent> components)
    {
        var taxIds = components.Where(c => c.Type == SalaryComponentType.Tax).Select(c => (Guid?)c.ComponentId).ToHashSet();
        var earnings = lines.Where(l => l.LineType == PayslipLineType.Earning).ToList();
        var deductionLines = lines.Where(l => l.LineType == PayslipLineType.Deduction).ToList();

        var gross = earnings.Sum(l => l.Amount);
        var basicAmount = Round(basic);
        var tax = deductionLines.Where(l => taxIds.Contains(l.ComponentId)).Sum(l => l.Amount);
        var deductions = deductionLines.Sum(l => l.Amount) - tax;
        var net = Math.Max(0, gross - deductions - tax);

        return new SalaryBreakdown(basicAmount, gross - basicAmount, gross, deductions, tax, net, lines);
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
