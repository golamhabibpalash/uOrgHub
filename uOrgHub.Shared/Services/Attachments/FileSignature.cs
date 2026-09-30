namespace uOrgHub.Shared.Services.Attachments;

/// <summary>
/// Checks a file's leading "magic" bytes against its extension, so a renamed file (an executable
/// saved as <c>invoice.pdf</c>) is rejected rather than stored and later served inline as a PDF or
/// image. Only formats that preview inline in the browser — and so matter most — are checked;
/// text and Office formats have no reliable single signature and pass through on extension alone.
/// </summary>
public static class FileSignature
{
    private static readonly byte[] Pdf = "%PDF"u8.ToArray();
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] Gif = "GIF8"u8.ToArray();
    private static readonly byte[] Riff = "RIFF"u8.ToArray();
    private static readonly byte[] Webp = "WEBP"u8.ToArray();

    /// <summary>Number of bytes <see cref="Matches"/> needs to see.</summary>
    public const int HeaderLength = 12;

    public static bool Matches(string extension, ReadOnlySpan<byte> header) => extension switch
    {
        ".pdf" => header.StartsWith(Pdf),
        ".png" => header.StartsWith(Png),
        ".jpg" or ".jpeg" => header.StartsWith(Jpeg),
        ".gif" => header.StartsWith(Gif),
        ".webp" => header.StartsWith(Riff) && header.Length >= 12 && header[8..12].SequenceEqual(Webp),
        _ => true
    };
}
