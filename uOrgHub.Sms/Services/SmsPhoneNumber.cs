using System.Text.RegularExpressions;

namespace uOrgHub.Sms.Services;

/// <summary>
/// Recipient numbers: parsing a typed/pasted list, validating, and storing one canonical form —
/// digits only, with country code — so "01712-345678", "+8801712345678" and "8801712345678" are the
/// same recipient (and de-duplicated). Providers format it further for their own API.
/// </summary>
public static partial class SmsPhoneNumber
{
    // Bangladeshi mobile: 01 + operator digit 3–9 + 8 digits, with or without 88 / +88 / 0088.
    [GeneratedRegex(@"^(?:\+?88|0088)?(01[3-9]\d{8})$")]
    private static partial Regex BangladeshMobile();

    // Any other country, written in international form: + then 8–15 digits.
    [GeneratedRegex(@"^\+([1-9]\d{7,14})$")]
    private static partial Regex International();

    [GeneratedRegex(@"[\s\-().]")]
    private static partial Regex Separators();

    /// <summary>Canonical digits-only form, or null when <paramref name="raw"/> is not a valid mobile number.</summary>
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var compact = Separators().Replace(raw.Trim(), "");

        var bd = BangladeshMobile().Match(compact);
        if (bd.Success) return "88" + bd.Groups[1].Value;

        var intl = International().Match(compact);
        return intl.Success ? intl.Groups[1].Value : null;
    }

    /// <summary>Splits a list on commas, semicolons and line breaks.</summary>
    public static List<string> SplitList(string? input)
        => (input ?? string.Empty)
            .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

    /// <summary>Valid numbers (normalised, de-duplicated, in the order given) and the entries that were not valid.</summary>
    public static (List<string> Valid, List<string> Invalid) Parse(IEnumerable<string> entries)
    {
        var valid = new List<string>();
        var seen = new HashSet<string>();
        var invalid = new List<string>();
        foreach (var entry in entries.SelectMany(SplitList))
        {
            var number = Normalize(entry);
            if (number is null) invalid.Add(entry);
            else if (seen.Add(number)) valid.Add(number);
        }
        return (valid, invalid);
    }

    /// <summary>Bangladeshi local form (01XXXXXXXXX) for providers that want it; other numbers unchanged.</summary>
    public static string ToLocal(string normalized)
        => normalized.StartsWith("8801") && normalized.Length == 13 ? normalized[2..] : normalized;

    /// <summary>Masks the middle digits for logs: 8801712345678 → 88017****5678.</summary>
    public static string Mask(string number)
        => number.Length <= 8 ? new string('*', number.Length) : number[..5] + new string('*', number.Length - 9) + number[^4..];
}
