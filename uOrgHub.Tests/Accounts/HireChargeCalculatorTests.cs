using FluentAssertions;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Services;

namespace uOrgHub.Tests.Accounts;

public class HireChargeCalculatorTests
{
    private static DateTime D(int y, int m, int d) => new(y, m, d);

    [Fact]
    public void PerDay_ChargesEveryDayInTheRange_BothEndsInclusive()
    {
        // The agreed example: excavator on Project A at 5,000/day, run 10–25 Jan = 16 days.
        var charge = HireChargeCalculator.Calculate(HireRateUnit.PerDay, 5_000m, D(2026, 1, 1), null, D(2026, 1, 10), D(2026, 1, 25));

        charge!.Days.Should().Be(16);
        charge.Amount.Should().Be(80_000m);
        charge.FromDate.Should().Be(D(2026, 1, 10));
        charge.ToDate.Should().Be(D(2026, 1, 25));
    }

    [Fact]
    public void OnlyDaysOnSite_AreCharged_WhenDeployedPartWayThroughTheRange()
    {
        // Roller arrives on the 17th of a 10–25 run: 9 days, as in the agreed example.
        var charge = HireChargeCalculator.Calculate(HireRateUnit.PerDay, 3_000m, D(2026, 1, 17), null, D(2026, 1, 10), D(2026, 1, 25));

        charge!.Days.Should().Be(9);
        charge.Amount.Should().Be(27_000m);
        charge.FromDate.Should().Be(D(2026, 1, 17));
    }

    [Fact]
    public void ChargingStopsOnTheReturnDate()
    {
        var charge = HireChargeCalculator.Calculate(HireRateUnit.PerDay, 1_000m, D(2026, 1, 1), D(2026, 1, 12), D(2026, 1, 10), D(2026, 1, 25));

        charge!.Days.Should().Be(3); // 10, 11, 12
        charge.ToDate.Should().Be(D(2026, 1, 12));
    }

    [Fact]
    public void NoOverlap_MeansNoCharge()
    {
        HireChargeCalculator.Calculate(HireRateUnit.PerDay, 1_000m, D(2026, 2, 1), null, D(2026, 1, 10), D(2026, 1, 25))
            .Should().BeNull();
        HireChargeCalculator.Calculate(HireRateUnit.PerDay, 1_000m, D(2026, 1, 1), D(2026, 1, 5), D(2026, 1, 10), D(2026, 1, 25))
            .Should().BeNull();
    }

    [Fact]
    public void PerMonth_WholeMonth_ChargesExactlyTheMonthlyRate()
    {
        var charge = HireChargeCalculator.Calculate(HireRateUnit.PerMonth, 150_000m, D(2026, 1, 1), null, D(2026, 2, 1), D(2026, 2, 28));

        charge!.Days.Should().Be(28);
        charge.Amount.Should().Be(150_000m);
    }

    [Fact]
    public void PerMonth_IsProratedAgainstEachCalendarMonthsOwnLength()
    {
        // 25 Jan – 5 Feb: 7 days of January (/31) + 5 days of February (/28).
        var charge = HireChargeCalculator.Calculate(HireRateUnit.PerMonth, 150_000m, D(2026, 1, 1), null, D(2026, 1, 25), D(2026, 2, 5));

        charge!.Days.Should().Be(12);
        charge.Amount.Should().Be(Math.Round(150_000m * 7 / 31 + 150_000m * 5 / 28, 2, MidpointRounding.AwayFromZero));
    }

    [Fact]
    public void TimeOfDayIsIgnored()
    {
        var charge = HireChargeCalculator.Calculate(HireRateUnit.PerDay, 100m,
            new DateTime(2026, 1, 10, 18, 30, 0), null, new DateTime(2026, 1, 10, 9, 0, 0), new DateTime(2026, 1, 11, 1, 0, 0));

        charge!.Days.Should().Be(2);
        charge.Amount.Should().Be(200m);
    }
}
