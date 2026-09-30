using uOrgHub.Accounts.Models.Enums;

namespace uOrgHub.Accounts.Services;

/// <summary>What one deployment owes for a run's date range: the days it was on site, and the amount.</summary>
public record HireCharge(DateTime FromDate, DateTime ToDate, int Days, decimal Amount);

/// <summary>
/// Pure hire-charge maths — no database. All dates are whole days and both ends are inclusive:
/// a machine deployed on the 10th and returned on the 12th was on site 3 days.
/// </summary>
public static class HireChargeCalculator
{
    /// <summary>
    /// Charge for the overlap of the deployment [<paramref name="deployStart"/>, <paramref name="deployEnd"/>]
    /// with the run's range [<paramref name="rangeFrom"/>, <paramref name="rangeTo"/>]; null when they
    /// do not overlap. A monthly rate is prorated day by day against each calendar month's own length,
    /// so a range crossing a month end is priced correctly on both sides.
    /// </summary>
    public static HireCharge? Calculate(HireRateUnit unit, decimal rate,
        DateTime deployStart, DateTime? deployEnd, DateTime rangeFrom, DateTime rangeTo)
    {
        var from = Max(deployStart.Date, rangeFrom.Date);
        var to = deployEnd.HasValue ? Min(deployEnd.Value.Date, rangeTo.Date) : rangeTo.Date;
        if (from > to) return null;

        var days = (int)(to - from).TotalDays + 1;
        decimal amount;

        if (unit == HireRateUnit.PerDay)
        {
            amount = days * rate;
        }
        else
        {
            amount = 0m;
            // Walk month by month: each segment's days are priced at that month's daily share.
            for (var segStart = from; segStart <= to;)
            {
                var monthEnd = new DateTime(segStart.Year, segStart.Month, DateTime.DaysInMonth(segStart.Year, segStart.Month));
                var segEnd = Min(monthEnd, to);
                var segDays = (int)(segEnd - segStart).TotalDays + 1;
                amount += rate * segDays / DateTime.DaysInMonth(segStart.Year, segStart.Month);
                segStart = segEnd.AddDays(1);
            }
        }

        return new HireCharge(AsUtcDay(from), AsUtcDay(to), days, Math.Round(amount, 2, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Dates arrive with mixed kinds — deployment dates read back from the database as UTC, range
    /// dates from the query string as unspecified. Stamping every whole day as UTC keeps what the
    /// API returns (and stores) consistent.
    /// </summary>
    public static DateTime AsUtcDay(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}
