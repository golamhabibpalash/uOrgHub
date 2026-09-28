using FluentAssertions;
using uOrgHub.Accounts.Models.Enums;
using uOrgHub.Accounts.Services;

namespace uOrgHub.Tests.Accounts;

public class DepreciationCalculatorTests
{
    private static readonly DateTime Jan2026 = new(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    private static DepreciationCharge StraightLine(decimal cost, decimal salvage, int life, decimal accumulated,
        DateTime start, DateTime? last, int year, int month) =>
        DepreciationCalculator.Calculate(DepreciationMethod.StraightLine, cost, salvage, life, accumulated,
            start, last, DepreciationCalculator.MonthEnd(year, month));

    [Fact]
    public void StraightLine_ChargesOneMonthOfDepreciableAmount()
    {
        // Excavator: 12,000,000 cost, 600,000 salvage, 10 years → 95,000 a month.
        var charge = StraightLine(12_000_000m, 600_000m, 120, 0m, Jan2026, null, 2026, 1);

        charge.Months.Should().Be(1);
        charge.Amount.Should().Be(95_000m);
    }

    [Fact]
    public void NothingIsDue_BeforeTheStartMonth()
    {
        var charge = StraightLine(120_000m, 0m, 12, 0m, Jan2026, null, 2025, 12);

        charge.Months.Should().Be(0);
        charge.Amount.Should().Be(0m);
    }

    [Fact]
    public void NothingIsDue_WhenAlreadyChargedForThePeriod()
    {
        var charge = StraightLine(120_000m, 0m, 12, 10_000m, Jan2026, DepreciationCalculator.MonthEnd(2026, 1), 2026, 1);

        charge.Amount.Should().Be(0m);
    }

    [Fact]
    public void SkippedMonths_AreCaughtUpInTheNextRun()
    {
        // Last charged January; a run for April owes February, March and April.
        var charge = StraightLine(120_000m, 0m, 12, 10_000m, Jan2026, DepreciationCalculator.MonthEnd(2026, 1), 2026, 4);

        charge.Months.Should().Be(3);
        charge.Amount.Should().Be(30_000m);
    }

    [Fact]
    public void FinalMonth_SweepsUpRoundingSoTheAssetEndsExactlyAtSalvage()
    {
        // 100 / 3 = 33.33 a month; the third month must be 33.34, not 33.33.
        var charge = StraightLine(100m, 0m, 3, 0m, Jan2026, null, 2026, 3);

        charge.Months.Should().Be(3);
        charge.Amount.Should().Be(100m);
    }

    [Fact]
    public void Depreciation_NeverGoesBelowSalvage()
    {
        // Fully depreciated already: cost 1,000, salvage 100, accumulated 900.
        var charge = StraightLine(1_000m, 100m, 12, 900m, Jan2026, DepreciationCalculator.MonthEnd(2026, 12), 2027, 6);

        charge.Amount.Should().Be(0m);
    }

    [Fact]
    public void OpeningDepreciation_OnlyChargesMonthsAfterTheOpeningPoint()
    {
        // Bought Jan 2024, 60-month life, depreciated elsewhere up to Dec 2025 (24 months × 1,000).
        var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var charge = StraightLine(60_000m, 0m, 60, 24_000m, start, DepreciationCalculator.MonthEnd(2025, 12), 2026, 1);

        charge.Months.Should().Be(1);
        charge.Amount.Should().Be(1_000m);
    }

    [Fact]
    public void DecliningBalance_ChargesTwiceTheStraightLineRateOnBookValue()
    {
        // 2 / 120 of 1,200,000 = 20,000 in month one; month two is 2 / 120 of 1,180,000.
        var charge = DepreciationCalculator.Calculate(DepreciationMethod.DecliningBalance, 1_200_000m, 0m, 120, 0m,
            Jan2026, null, DepreciationCalculator.MonthEnd(2026, 2));

        charge.Months.Should().Be(2);
        charge.Amount.Should().Be(20_000m + Math.Round(1_180_000m * 2m / 120m, 2, MidpointRounding.AwayFromZero));
    }
}
