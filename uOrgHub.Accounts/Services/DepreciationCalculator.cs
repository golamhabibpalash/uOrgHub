using uOrgHub.Accounts.Models.Entities;
using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.Services;

/// <summary>The charge one asset is due for a period: how many months, and how much.</summary>
public record DepreciationCharge(int Months, decimal Amount);

/// <summary>
/// Pure monthly depreciation maths — no database, so it can be tested in isolation.
///
/// An asset is charged for every whole month from the month after its last charge (or from its
/// depreciation start month) up to and including the period being run. A run for a later month
/// therefore catches up any months that were skipped instead of silently losing them.
/// Depreciation never takes the book value below salvage, and the final month of the useful life
/// sweeps up whatever rounding left behind so the asset ends exactly at salvage.
/// </summary>
public static class DepreciationCalculator
{
    public static DateTime MonthEnd(int year, int month) =>
        new(year, month, DateTime.DaysInMonth(year, month), 0, 0, 0, DateTimeKind.Utc);

    public static DepreciationCharge Calculate(FixedAsset asset, DateTime periodEnd) =>
        Calculate(asset.DepreciationMethod, asset.PurchaseCost, asset.SalvageValue, asset.UsefulLifeMonths,
                  asset.AccumulatedDepreciation, asset.DepreciationStartDate, asset.LastDepreciationDate, periodEnd);

    public static DepreciationCharge Calculate(
        DepreciationMethod method,
        decimal cost,
        decimal salvage,
        int usefulLifeMonths,
        decimal accumulated,
        DateTime depreciationStart,
        DateTime? lastDepreciationDate,
        DateTime periodEnd)
    {
        if (usefulLifeMonths <= 0) return new DepreciationCharge(0, 0);

        var startMonth = MonthIndex(depreciationStart);
        var firstDueMonth = lastDepreciationDate.HasValue ? MonthIndex(lastDepreciationDate.Value) + 1 : startMonth;
        var lastDueMonth = MonthIndex(periodEnd);
        if (firstDueMonth > lastDueMonth) return new DepreciationCharge(0, 0);

        var depreciable = cost - salvage;
        var total = 0m;
        var months = 0;

        for (var m = firstDueMonth; m <= lastDueMonth; m++)
        {
            var remaining = depreciable - accumulated - total;
            if (remaining <= 0) break;

            // 1-based position of this month within the asset's useful life.
            var lifeMonth = m - startMonth + 1;

            var charge = lifeMonth >= usefulLifeMonths
                ? remaining
                : method switch
                {
                    DepreciationMethod.DecliningBalance => (cost - accumulated - total) * 2m / usefulLifeMonths,
                    _ => depreciable / usefulLifeMonths,
                };

            charge = Math.Min(Math.Round(charge, 2, MidpointRounding.AwayFromZero), remaining);
            if (charge <= 0) break;

            total += charge;
            months++;
        }

        return new DepreciationCharge(months, total);
    }

    private static int MonthIndex(DateTime d) => d.Year * 12 + d.Month - 1;
}
