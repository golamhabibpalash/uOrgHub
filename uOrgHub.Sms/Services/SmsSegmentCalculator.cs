using uOrgHub.Sms.Models.Enums;

namespace uOrgHub.Sms.Services;

/// <summary>How a message text splits into SMS segments.</summary>
/// <param name="Units">
/// Size in the encoding's own units: septets for GSM-7 (an extension character such as € or [
/// costs two), UTF-16 code units for Unicode (an emoji costs two).
/// </param>
/// <param name="PerSegmentLimit">Units one segment holds: 160/70 for a single SMS, 153/67 per part once split.</param>
/// <param name="RemainingInSegment">Units left before the next segment starts.</param>
public record SmsSegmentInfo(
    SmsEncoding Encoding, int Characters, int Units, int Segments, int PerSegmentLimit, int RemainingInSegment);

/// <summary>
/// Counts SMS segments exactly as a handset or gateway does (3GPP TS 23.038 / 23.040).
///
/// A message that is all GSM 03.38 characters goes as GSM-7: 160 septets in one SMS, or 153 per
/// part once it needs more than one (the user-data header takes 7). Anything else — Bangla,
/// emoji, curly quotes — makes the whole message UCS-2: 70 code units, or 67 per part.
///
/// Splitting is not plain division. A GSM extension character is an escape plus a septet and
/// a UTF-16 surrogate pair is two units; neither may be cut across two parts, so a part that
/// would end mid-character ends one unit early instead. That is why the count is built by
/// packing characters into parts rather than by ceiling(length / 153).
///
/// Mirrored in uOrgHub.Web/src/utils/smsSegments.ts — keep the two in step.
/// </summary>
public static class SmsSegmentCalculator
{
    private const string GsmBasic =
        "@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?" +
        "¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà";

    private const string GsmExtension = "^{}\\[~]|€\f";

    private static readonly HashSet<char> Basic = GsmBasic.ToHashSet();
    private static readonly HashSet<char> Extension = GsmExtension.ToHashSet();

    public const int GsmSingle = 160, GsmMulti = 153, UnicodeSingle = 70, UnicodeMulti = 67;

    public static SmsEncoding DetectEncoding(string text)
        => text.All(c => Basic.Contains(c) || Extension.Contains(c)) ? SmsEncoding.Gsm7 : SmsEncoding.Unicode;

    public static SmsSegmentInfo Calculate(string? text)
    {
        text ??= string.Empty;
        var encoding = DetectEncoding(text);
        var costs = encoding == SmsEncoding.Gsm7 ? GsmCosts(text) : UnicodeCosts(text);
        var characters = costs.Count;
        var units = costs.Sum();

        var (single, multi) = encoding == SmsEncoding.Gsm7 ? (GsmSingle, GsmMulti) : (UnicodeSingle, UnicodeMulti);

        if (units == 0)
            return new SmsSegmentInfo(encoding, 0, 0, 0, single, single);
        if (units <= single)
            return new SmsSegmentInfo(encoding, characters, units, 1, single, single - units);

        var segments = 1;
        var used = 0;
        foreach (var cost in costs)
        {
            if (used + cost > multi)
            {
                segments++;
                used = 0;
            }
            used += cost;
        }
        return new SmsSegmentInfo(encoding, characters, units, segments, multi, multi - used);
    }

    /// <summary>One entry per character: 1 septet, or 2 for an extension-table character.</summary>
    private static List<int> GsmCosts(string text)
        => text.Select(c => Extension.Contains(c) ? 2 : 1).ToList();

    /// <summary>One entry per character: 1 code unit, or 2 for a surrogate pair (kept together).</summary>
    private static List<int> UnicodeCosts(string text)
    {
        var costs = new List<int>(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                costs.Add(2);
                i++;
            }
            else
            {
                costs.Add(1);
            }
        }
        return costs;
    }
}
