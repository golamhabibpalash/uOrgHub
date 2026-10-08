using FluentAssertions;
using uOrgHub.HR.Features.Payroll;
using uOrgHub.HR.Models.Enums;

namespace uOrgHub.Tests.HR;

public class PayrollCalculatorTests
{
    private static readonly DateTime Start = new(2026, 9, 1);
    private static readonly DateTime End = new(2026, 9, 30); // 30 days

    // Basic 30,000 + house rent 50% of basic + medical 1,500 fixed; PF 5% of basic; tax 1,000.
    private static readonly StructureComponent[] Components =
    [
        new(Guid.NewGuid(), "HRA", "House Rent", SalaryComponentType.HouseRentAllowance, CalculationType.PercentageOfBasic, 50, 1),
        new(Guid.NewGuid(), "MED", "Medical", SalaryComponentType.MedicalAllowance, CalculationType.Fixed, 1500, 2),
        new(Guid.NewGuid(), "PF", "Provident Fund", SalaryComponentType.PF, CalculationType.PercentageOfBasic, 5, 3),
        new(Guid.NewGuid(), "TAX", "Income Tax", SalaryComponentType.Tax, CalculationType.Fixed, 1000, 4),
    ];

    private static PayrollInput Input(int absent = 0, decimal unpaidLeave = 0, decimal overtimeHours = 0,
        OvertimePolicy? overtime = null, DateTime? joining = null) =>
        new(Start, End, joining ?? new DateTime(2025, 1, 1), 30000, Components,
            PresentDays: 20, AbsentDays: absent, PaidLeaveDays: 0, UnpaidLeaveDays: unpaidLeave,
            OvertimeHours: overtimeHours, Overtime: overtime);

    [Fact]
    public void Monthly_salary_sums_basic_allowances_and_deductions()
    {
        var m = PayrollCalculator.Monthly(30000, Components);

        m.Basic.Should().Be(30000);
        m.Allowances.Should().Be(16500);   // 15,000 HRA + 1,500 medical
        m.Gross.Should().Be(46500);
        m.Deductions.Should().Be(1500);    // PF
        m.Tax.Should().Be(1000);
        m.Net.Should().Be(44000);
    }

    [Fact]
    public void Full_month_with_no_absence_pays_monthly_net()
    {
        var r = PayrollCalculator.Calculate(Input());

        r.TotalDays.Should().Be(30);
        r.UnpaidDays.Should().Be(0);
        r.Net.Should().Be(44000);
        r.Lines.Should().Contain(l => l.Code == "BASIC" && l.Amount == 30000);
    }

    [Fact]
    public void Absent_and_unpaid_leave_days_are_deducted_from_gross()
    {
        var r = PayrollCalculator.Calculate(Input(absent: 2, unpaidLeave: 1));

        r.UnpaidDays.Should().Be(3);
        // 46,500 / 30 × 3 = 4,650
        r.Lines.Should().Contain(l => l.Code == "UNPAID" && l.Amount == 4650);
        r.Deductions.Should().Be(1500 + 4650);
        r.Net.Should().Be(44000 - 4650);
    }

    [Fact]
    public void Mid_month_joiner_is_paid_only_from_joining_date()
    {
        var r = PayrollCalculator.Calculate(Input(joining: new DateTime(2026, 9, 11))); // 10 days before joining

        r.UnpaidDays.Should().Be(10);
        r.Lines.Should().Contain(l => l.Code == "UNPAID" && l.Amount == 15500);
    }

    [Fact]
    public void Overtime_is_paid_on_basic_hourly_rate_and_capped()
    {
        var rule = new OvertimePolicy(CalculationType.PercentageOfBasic, Multiplier: 2, MaxHoursPerMonth: 10);
        var r = PayrollCalculator.Calculate(Input(overtimeHours: 12.5m, overtime: rule));

        // 30,000 / 208 × 2 × 10 h (capped) = 2,884.62
        r.OvertimePay.Should().Be(2884.62m);
        r.Gross.Should().Be(46500 + 2884.62m);
        r.Allowances.Should().Be(16500);
    }

    [Fact]
    public void Overtime_hours_without_a_rule_are_not_paid()
    {
        var r = PayrollCalculator.Calculate(Input(overtimeHours: 5));

        r.OvertimePay.Should().Be(0);
    }

    [Fact]
    public void Net_never_goes_negative()
    {
        var r = PayrollCalculator.Calculate(Input(absent: 30));

        r.Net.Should().Be(0);
    }
}
