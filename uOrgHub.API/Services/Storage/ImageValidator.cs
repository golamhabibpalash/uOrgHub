using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata;

namespace uOrgHub.API.Services.Storage;

public class ImageValidationOptions
{
    public long MaxBytes { get; init; } = 5 * 1024 * 1024;
    public HashSet<string> AllowedContentTypes { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp"
    };
    public int MinWidth { get; init; } = 100;
    public int MinHeight { get; init; } = 100;
    public int MaxWidth { get; init; } = 4096;
    public int MaxHeight { get; init; } = 4096;
}

public record ImageValidationResult(bool IsValid, string? Error, string? FormatName, int Width, int Height);

public static class ImageValidator
{
    public static async Task<ImageValidationResult> ValidateAsync(
        Stream stream,
        string contentType,
        long size,
        ImageValidationOptions? options = null)
    {
        options ??= new ImageValidationOptions();

        if (size <= 0)
            return Fail("File is empty.");

        if (size > options.MaxBytes)
            return Fail($"File is too large. Maximum allowed size is {options.MaxBytes / (1024 * 1024)} MB.");

        if (!options.AllowedContentTypes.Contains(contentType))
            return Fail($"Unsupported file type '{contentType}'. Allowed types: {string.Join(", ", options.AllowedContentTypes)}.");

        if (stream.CanSeek) stream.Position = 0;

        try
        {
            var image = await Image.IdentifyAsync(stream);
            if (image is null)
                return Fail("Could not read image. The file may be corrupt or not a valid image.");

            if (image.Width < options.MinWidth || image.Height < options.MinHeight)
                return Fail($"Image dimensions too small. Minimum is {options.MinWidth}x{options.MinHeight}px.");

            if (image.Width > options.MaxWidth || image.Height > options.MaxHeight)
                return Fail($"Image dimensions too large. Maximum is {options.MaxWidth}x{options.MaxHeight}px.");

            return new ImageValidationResult(true, null, image.Metadata.DecodedImageFormat?.Name, image.Width, image.Height);
        }
        catch (UnknownImageFormatException)
        {
            return Fail("The file is not a recognized image format. Allowed: JPEG, PNG, WEBP.");
        }
        catch (InvalidImageContentException)
        {
            return Fail("The image content is invalid or corrupted.");
        }
        finally
        {
            if (stream.CanSeek) stream.Position = 0;
        }
    }

    private static ImageValidationResult Fail(string error) =>
        new(false, error, null, 0, 0);
}
