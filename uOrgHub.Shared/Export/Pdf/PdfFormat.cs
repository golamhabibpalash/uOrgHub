using System.Globalization;

namespace uOrgHub.Shared.Export.Pdf;

/// <summary>Consistent value formatting shared by every report PDF builder.</summary>
public static class PdfFormat
{
    public static string Amount(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);

    public static string AmountOrBlank(decimal value) => value == 0 ? "-" : Amount(value);

    public static string Date(DateTime value) => value.ToString("yyyy-MM-dd");

    public static string Date(DateTime? value) => value?.ToString("yyyy-MM-dd") ?? "";

    /// <summary>
    /// Day-first date (31/12/2026), the way the app shows dates on screen. Invariant culture,
    /// because "/" in a format string is the culture's date separator, not a literal slash.
    /// </summary>
    public static string DayFirstDate(DateTime value) => value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string Bool(bool value) => value ? "Yes" : "No";
}
